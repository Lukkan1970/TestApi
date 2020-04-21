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

    let CheckIfDeviceIdExist (blobs:BlobResultSegment) (deviceId:string)  =
        let temp = blobs.Results
        let temp2 = List.ofSeq temp
        temp2
        |> List.map(fun x -> x :?> CloudBlob)
        |> List.tryFind(fun x -> x.Name = deviceId)


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
       // let resultSegment2 = ListBlobsSegmentedInFlatListing myList.[0] |> Async.RunSynchronously
        //let resultSegment = 
        //    myCont.ListBlobsSegmentedAsync(
        //        "", true, BlobListingDetails.All, Nullable 10, 
        //        null, null, null, Threading.CancellationToken.None) 
        //    |> Async.AwaitTask
        
        let test = myCont.ListBlobsSegmentedAsync("", true, BlobListingDetails.Metadata, Nullable 10, null, null, null)
        let myBlobs = test |> Async.AwaitTask |> Async.RunSynchronously

        let firstCheck = CheckIfDeviceIdExist

        
        value
       

       //.ListBlobsSegmentedAsync(string.Empty, true, BlobListingDetails.Metadata, null, null, null, null);

       //  BlobResultSegment
       

    

    

