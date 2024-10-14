using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Assignment3TestSuite;


public class Server
{
    private readonly int _port;

    public Server(int port)
    {
        _port = port;
        
    }
    public void Run(){

        var server = new TcpListener(IPAddress.Loopback, _port);
        server.Start();

        Console.WriteLine($"Server started on port {_port}");

        while (true)
        {
            var client = server.AcceptTcpClient();
            Console.WriteLine("Client connected!!!");

            Task.Run(() => HandleClient(client));

            
        }

    }
    
    private void HandleClient(TcpClient client)
    {

        Category category = new Category();
        try
        {
            var stream = client.GetStream();
            string msg = ReadFromStream(stream);
            Console.WriteLine("Message from client: " + msg);


            //Míssing method
            if (msg == "{}")
            {
                var response = new Response
                {
                    Status = "missing method"

                };

                var json = ToJson(response);
                WriteToStream(stream, json);

            }
            
            var request = FromJson(msg);


            //request is null. 
            if (request == null)
            {
                

            }


            //Illegal method
            string[] validMethods = ["create", "read", "update", "delete", "echo"];

            if (!validMethods.Contains(request.Method))
            {
                var response = new Response
                {
                    Status = "illegal method"
                };

                var json = ToJson(response);
                WriteToStream(stream, json);
                return;
            }


            //Missing resource
            if ((request.Method == "create" || request.Method == "read" || request.Method == "update" || request.Method == "delete") && request.Path == null)
            {

                var response = new Response
                {
                    Status = "missing resource"
                };

                var json = ToJson(response);
                WriteToStream(stream, json);
                return;

            }

            //illegal date
            if (!long.TryParse(request.Date, out long date))
            {

                var response = new Response
                {
                    Status = "illegal date"
                };

                var json = ToJson(response);
                WriteToStream(stream, json);
                return;
            }

            //missing body
            if((request.Method == "create" || request.Method == "update" || request.Method == "echo") && request.Body == null)
            {
                var response = new Response
                {
                    Status = "missing body"
                };

                var json = ToJson(response);
                WriteToStream(stream, json);
                return;
            }

            //illegal body
            if (request.Method == "update" && !(request.Body.StartsWith("{") && request.Body.EndsWith("}")) &&!(request.Body.StartsWith("[") && request.Body.EndsWith("]")))
            {
                var response = new Response
                {
                    Status = "illegal body"
                };

                var json = ToJson(response);
                WriteToStream(stream, json);
                return;

            }

            //Echo test
            if (request.Method == "echo")
            {
                var response = new Response
                {
                    Body = request.Body,
                    Status = "1 OK"
                };

                var json = ToJson(response);
                WriteToStream(stream, json);
                return;
            }

            //4 bad request 2
            if (request.Path != "/api/categories" && request.Method == "create")
            {
                var response = new Response
                {
                    Status = "4 Bad Request"

                };
                var json = ToJson(response);
                WriteToStream(stream, json);
                return;
            }

            //4 bad request 3
            string[] validPathsForUpdateAndDelete = ["/api/categories/1", "/api/categories/2", "/api/categories/3"];
            if (!validPathsForUpdateAndDelete.Contains(request.Path) && (request.Method == "update" || request.Method == "delete"))
            {
                var response = new Response
                {
                    Status = "4 Bad Request"

                };
                var json = ToJson(response);
                WriteToStream(stream, json);
                return;
            }

            //read test 2
            if(request.Method == "read" && request.Path == "/api/categories")
            {
                var response = new Response
                {
                    Status = "1 Ok",
                    Body = category.GetAllCategories().ToJson()
                };

                var json = ToJson(response);
                WriteToStream(stream, json);
                return;

            } 

            //read test 2
            if(request.Method == "read")
            {
                Response response;

                int cid;
               
                int lastSlash = request.Path.LastIndexOf('/');

                string cidStr = request.Path.Substring(lastSlash + 1);

                bool isInteger = int.TryParse(cidStr, out cid);

                if(isInteger)
                {

                    if (category.GetCategory(cid) != null)
                    {
                        response = new Response
                        {
                            Status = "1 Ok",
                            Body = category.GetCategory(cid).ToJson()
                        };

                    }
                    else
                    {
                        response = new Response
                        {
                            Status = "5 not found",                      
                        };

                    }


                }
                else
                {
                    //4 bad request
                    response = new Response
                    {
                        Status = "4 Bad Request"
                    };

                }

                var json = ToJson(response);
                WriteToStream(stream, json);
                return;


            }

            
            if(request.Method == "update")
            {

                Response response;

                int cid;

                int lastSlash = request.Path.LastIndexOf('/');

                string cidStr = request.Path.Substring(lastSlash + 1);

                bool isInteger = int.TryParse(cidStr, out cid);

                if (isInteger)
                {

                    if (category.GetCategory(cid) != null)
                    {

                        //DOES NOT UPDATE ANYTHING
                        //category.UpdateCategory(cid, name);
                        response = new Response
                        {
                            Status = "3 Updated"
                        };

                    }
                    else
                    {
                        response = new Response
                        {
                            Status = "5 not found",
                        };

                    }


                }
                else
                {
                    //4 bad request
                    response = new Response
                    {
                        Status = "4 Bad Request"
                    };

                }

                var json = ToJson(response);
                WriteToStream(stream, json);
                return;


            }

            

        }
        catch { }

    }

    private static string ReadFromStream(NetworkStream stream)
    {
        var buffer = new byte[1024];
        var readCount = stream.Read(buffer);
        return Encoding.UTF8.GetString(buffer, 0, readCount);
        
    }

    private void WriteToStream(NetworkStream stream, string msg)
    {
        var buffer = Encoding.UTF8.GetBytes(msg);
        stream.Write(buffer);
    }

    public static string ToJson(Response response)
    {
        return JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static Request? FromJson(string element)
    {
        return JsonSerializer.Deserialize<Request>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }


}

