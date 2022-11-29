#nullable enable
namespace Shop.Api.Entities{


    public class ResponseIgmur {
       public ResponseIgmurData data {get; set;}
    }

    public class ResponseIgmurData {
       public string id {get; set;} = "";

       public string type {get; set;} = "";

       public string link {get; set;} = "";
    }
        
        

}