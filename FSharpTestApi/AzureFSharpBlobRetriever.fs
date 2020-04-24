namespace FSharpTestApi

open FSharp.Data
open System
open System.IO
open Microsoft.Azure // Namespace for CloudConfigurationManager
//open Microsoft.Azure.Storage // Namespace for CloudStorageAccount
//open Microsoft.Azure.Storage.Blob // Namespace for Blob storage types

open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob

open System.IO.Compression

module FSharpTestApiModule =

    let accountName = "sigmaiotexercisetest";
    let SAS = "?sv=2017-11-09&ss=bfqt&srt=sco&sp=rl&se=2028-09-27T16:27:24Z&st=2018-09-27T08:27:24Z&spr=https&sig=eYVbQneRuiGn103jUuZvNa6RleEeoCFx1IftVin6wuA%3D";
    
    [<Literal>]
    let loadExampleValues = __SOURCE_DIRECTORY__ + "./Example_Data/Example.json"
    
    [<Literal>]
    let sensorvalue =  __SOURCE_DIRECTORY__ + "./Example_Data/2019-01-11_rainfall.csv"
    
    [<Literal>]
    let resultdata =  __SOURCE_DIRECTORY__ + "./Example_Data/Example.json"
    
    type ResultData = JsonProvider<resultdata>
    
    type SensorResult = CsvProvider<sensorvalue>

    let GetContainerAsync (client:CloudBlobClient) = 
        let response = client.ListContainersSegmentedAsync(null) |> Async.AwaitTask
        response

    let CheckIfDeviceIdExist (blobs:IListBlobItem list) (deviceId:string)  =
        blobs
        |> List.map(fun x -> x :?> CloudBlob)
        |> List.filter (fun x -> x.Name.StartsWith(deviceId))

    let GetSensorTypesForDevice (blobs:CloudBlob list) (deviceId:string)  =
        blobs
        |> List.map(fun x -> x.Name)
        |> List.map(fun x -> (x.Split("/")).[1])
        |> List.distinct

    let GetCheckSensorType (sensorTypes: string list) (sensorType: string) =
        let containsSensorType = sensorTypes |> List.contains sensorType
        if sensorType <> null && not containsSensorType
        then false
        else true
        
    let  GetSelectedSensorTypes  (sensorTypes: string list) (sensorType: string) =
        match sensorType with
        | null -> sensorTypes
        | _ -> [sensorType]

    let GetZipFileFromAzureAsync (container: CloudBlobContainer) (date: string) (zipPath: string) = async {
        use blobStream = new MemoryStream()
        let zipBlob = container.GetBlockBlobReference(zipPath)
        let temp = zipBlob.DownloadToStreamAsync(blobStream) |> Async.AwaitTask |> Async.RunSynchronously

        use zip =  new ZipArchive(blobStream)
        let entryName = date + ".csv"
        let entry = zip.GetEntry(entryName)
            
        if entry <> null
        then
            use sr = new StreamReader(entry.Open())
            let! zipText = sr.ReadToEndAsync() |> Async.AwaitTask
            return zipText
        else
            let noData = (ResultData.Root("No data found that date", null)).JsonValue
            return noData.ToString()
        }

    let GetFileFromAzureAsync (container: CloudBlobContainer) (deviceId:string) (date: string) (sensorType: string ) = async {
        let text = String.Empty
        let blobPath = deviceId + "/" + sensorType + "/" + date + ".csv"
        let zipPath = deviceId + "/" + sensorType + "/historical.zip"
        if (container.GetAppendBlobReference(blobPath).ExistsAsync() |> Async.AwaitTask |> Async.RunSynchronously)
        then 
            let blob = container.GetAppendBlobReference(blobPath) 
            let! csvText = blob.DownloadTextAsync() |> Async.AwaitTask
            return sensorType, csvText
        else if (container.GetBlockBlobReference(zipPath).ExistsAsync() |> Async.AwaitTask |> Async.RunSynchronously)
        then
            let! zipText  = (GetZipFileFromAzureAsync container date zipPath) 
            return sensorType, zipText
        else
            let noData = (ResultData.Root("No data found that date", null)).JsonValue
            return sensorType, noData.ToString()
        }
        

    let DownloadFiles (container: CloudBlobContainer) (deviceId:string) (date: string) (sensorTypes: string list) = 
        //sensorTypes
        let test = GetFileFromAzureAsync container deviceId date sensorTypes.[0]
        let testRes = test |> Async.RunSynchronously
        let splitter = [Environment.NewLine]
        let multi = 
            sensorTypes
            |> List.map(fun x -> GetFileFromAzureAsync container deviceId date x)
            |> Async.Parallel
            |> Async.RunSynchronously
            |> Array.map(fun x ->(fst x), (snd x).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            |> Array.map(fun x -> (fst x), (snd x) |> Array.map(fun y -> y.Split(';')))
            |> Array.map(fun x -> snd x |> Array.map (fun y -> fst x, y.[0], y.[1]))
        multi

    let ReorganizeOneFile (deviceID : string) (myFile: ( string * string * string)  []) =
        //let mySensor = "mySensor"
        //let myString = "myString"
        let all = 
            myFile
            |> Array.map (fun (sensor, date, value) ->  ResultData.MeasuredValue(DateTime.Parse date, [|ResultData.SensorValue(sensor, ResultData.DecimalOrString(value))|] ))
        //let myDecimalOrString = ResultData.DecimalOrString(myString)
        //let mySensorValue = [|ResultData.SensorValue(mySensor, myDecimalOrString)|]
        //let myDate = DateTime.Now
        //let myMeasuredValue = [|ResultData.MeasuredValue(myDate, mySensorValue)|]
        let temp = all
        let myName = deviceID
        let myRoot = ResultData.Root( myName, all) 
        myRoot

    let zipAllFiles (firstFile: ResultData.Root ) (allFiles : (string*string*string)[] ) = 
        let tempMes : ResultData.MeasuredValue [] = firstFile.MeasuredValues

        //let myFile = tempMes |>Array.take 10 |> Array.map(fun x -> x.SensorValues) 
        let myFile = tempMes |> Array.map(fun x -> x.SensorValues) 

        
        let secondFile =
            allFiles
            |> Array.map(fun (sensor, _, value) -> ResultData.SensorValue(sensor, ResultData.DecimalOrString(value)))


        let mapped = Array.map2(fun (x: ResultData.SensorValue[]) (y: ResultData.SensorValue) -> Array.append x [|y|] ) myFile secondFile
        
        //let newFile = Array.map2(fun x y -> ResultData.MeasuredValue(x.Date, y) ) tempMes mapped
        let newFile = Array.map2(fun (x:ResultData.MeasuredValue)  (y:ResultData.SensorValue []) -> 
                            let myDate : DateTime = x.Date
                            let mSensoorValue = y
                            ResultData.MeasuredValue(myDate, mSensoorValue)
                            ) (tempMes : ResultData.MeasuredValue []) (mapped : ResultData.SensorValue [][])
        ResultData.Root(firstFile.Name, newFile)
        

    let ReorgnanizeFiles (deviceID : string) (myFiles: (string*string*string)[] list)  = 
        let firstFile = (ReorganizeOneFile deviceID myFiles.[0] )
       
        //let allZipedFiles = zipAllFiles firstFile myFiles
        let allZipedFiles =List.fold(fun acc elem -> zipAllFiles acc elem) firstFile (myFiles |> List.skip 1)
        //let secondFile = zipAllFiles firstFile myFiles.[1] 
        //let thirdFile = zipAllFiles secondFile myFiles.[2] 
        allZipedFiles
        //myFiles
        //|> List.map (ReorganizeOneFile deviceID)

    let GetMeasuredValuesAsync (deviceId: string) (date:string) (sensorType:string) = 
        let result = ResultData.GetSample
        let sensor = SensorResult.GetSample
        let value = JsonValue.Load(loadExampleValues)
        let sc = Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(SAS)
        let csa = Microsoft.WindowsAzure.Storage.CloudStorageAccount(sc, accountName, null, true)

        let _client = csa.CreateCloudBlobClient()

        let container = (GetContainerAsync _client)
        let container2 = (GetContainerAsync _client) |> Async.RunSynchronously
        let resultSegment =  container2.Results 
        let myList = List.ofSeq resultSegment
        let myCont = myList.[0]
     
        let test = myCont.ListBlobsSegmentedAsync("", true, BlobListingDetails.Metadata, System.Nullable(), null, null, null)
        let myBlobs = (test |> Async.AwaitTask |> Async.RunSynchronously).Results |> List.ofSeq

        let firstCheck = CheckIfDeviceIdExist myBlobs deviceId

        let wrongDeviceID = (ResultData.Root("DeviceId does not exist", null)).JsonValue

        let wrongSensorType = (ResultData.Root("SensorType does not exist", null)).JsonValue

        let sensorTypes = GetSensorTypesForDevice firstCheck deviceId

        let checkSensorType = GetCheckSensorType sensorTypes sensorType

        let selectedSensorTypes = GetSelectedSensorTypes sensorTypes sensorType

        let myFiles = (DownloadFiles myCont deviceId date selectedSensorTypes) |> Array.toList

        let myParse2 = myFiles.[0]

        let sec = myParse2.ToString()
        //let secsec = sec.[0].ToString()

       // let myValue = JsonValue.Parse( sec )

       // let myParse = ResultData.Parse(myFiles.[0])

        //let temp2 = SensorResult.Parse(myFiles.[0])

        let reorganizedFiles = ReorgnanizeFiles deviceId myFiles

        match firstCheck.Length, checkSensorType with
        | 0, _ -> wrongDeviceID
        | _, false -> wrongSensorType
        | _, _ -> reorganizedFiles.JsonValue
        

    

    

