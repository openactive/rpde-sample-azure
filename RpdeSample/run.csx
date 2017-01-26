#r "System.Configuration"
#r "System.Data"
#r "Newtonsoft.Json"

using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;

private class ProductData
{
    public string name { get; set; }
    public string productNumber { get; set; }
    public string color { get; set; }
}

private class Item
{
    public string state { get; set; }
    public string kind { get; set; }
    public string id { get; set; }
    public string modified { get; set; }
    public ProductData data { get; set; }
}

private class RpdeBody
{
    public string next { get; set; }
    public List<Item> items { get; set; }
    public string license { get; set; }
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // parse query parameter
    string code = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "code", true) == 0)
        .Value;

    // parse query parameters 'afterTimestamp' and 'afterId'
    string afterTimestamp = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "afterTimestamp", true) == 0)
        .Value;

    // note for implementations where SQL Server's rowversion is used
    string afterId = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "afterId", true) == 0)
        .Value;

    var lastTimestamp = afterTimestamp;
    var lastId = afterId;

  
    var items = new List<Item>();

    var str = ConfigurationManager.ConnectionStrings["sqldb_connection"].ConnectionString;
    using (SqlConnection conn = new SqlConnection(str))
    {
        string queryString =
            "SELECT TOP 5 PRODUCTID, CONVERT(VARCHAR(33), MODIFIEDDATE, 126), NAME, PRODUCTNUMBER, COLOR FROM SalesLT.Product "
                + (afterTimestamp != null ? " WHERE " : "")
                + (afterTimestamp != null && afterId == null ? " MODIFIEDDATE > Convert(varchar(30),@afterTimestamp,102) " : "")
                + (afterTimestamp != null && afterId != null ? " (MODIFIEDDATE = Convert(varchar(30),@afterTimestamp,102) AND PRODUCTID > @afterId) OR (MODIFIEDDATE > Convert(varchar(30),@afterTimestamp,102))  " : "")
                + "ORDER BY MODIFIEDDATE, PRODUCTID;";

        conn.Open();
        using (SqlCommand cmd = new SqlCommand(queryString, conn))
        {
            if (afterTimestamp != null) cmd.Parameters.AddWithValue("@afterTimestamp", afterTimestamp);
            if (afterId != null) cmd.Parameters.AddWithValue("@afterId", Int32.Parse(afterId));

            SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                 log.Info($"\t{reader[0]}\t{reader[1]}\t{reader[2]}\t{reader[3]}\t{reader[4]}");

                 var productData = new ProductData();
                 productData.name = $"{reader[2]}";
                 productData.productNumber = $"{reader[3]}";
                 productData.color = $"{reader[4]}";

                 var isDeleted = false; //TODO: Handle deleted flag here

                 var item = new Item();
                 item.id = $"{reader[0]}";
                 item.modified = $"{reader[1]}";
                 item.data = productData;
                 item.state = isDeleted ? "deleted" : "updated";
                 item.kind = "product";

                 lastTimestamp = item.modified;
                 lastId = item.id;

                 items.Add(item);
            }
            reader.Close();
        }
    }

    var RpdeBody = new RpdeBody();
    RpdeBody.items = items;
    RpdeBody.next = "/api/rpde?afterTimestamp=" + lastTimestamp + "&afterId=" + lastId;
    RpdeBody.license = "https://creativecommons.org/licenses/by/4.0/";

    var e = JsonConvert.SerializeObject(RpdeBody);

    //log.Info(e);

    var resp = afterTimestamp == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameters")
        : req.CreateResponse(HttpStatusCode.OK);

    resp.Content =  new StringContent(e, Encoding.UTF8, "application/json");

    return resp;
}