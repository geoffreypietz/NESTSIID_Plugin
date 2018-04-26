using Firebase.Database;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace HSPI_NESTSIID.Models
{
    class NestConnection : IDisposable
    {
        private string access_token;
        private const string client_id = "7ba01588-498d-4f20-a524-14c3c8f9134a";
        private const string client_secret = "b07sEmR7EJIv1TxXjQonuSyJq";


        public NestConnection()
        {

        }

        public void setInitialConnectionProps()
        {
            string json = Util.hs.GetINISetting("NEST", "login", "", Util.IFACE_NAME + ".ini");
            using (var Login = JsonConvert.DeserializeObject<Login>(json))
            {
                access_token = Login?.access_token; 
            }
        }

        private RestRequest getRequestGetOrPut(Method method, string json, bool stream)
        {
            var request = new RestRequest(method);
            request.RequestFormat = DataFormat.Json;
            //request.AddHeader("postman-token", "acc2297d-f543-b2d3-79f1-0a2c4536e4ae");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bearer " + access_token);
            request.AddHeader("content-type", "application/json");
            if (stream)
            {
                request.AddHeader("Accept", "text/event-stream");
            }
            if (json != null)
            {
                request.AddParameter("text/json", json, ParameterType.RequestBody);
            }
            return request;
        }

        private void saveLogin()
        {
            using (var login = new Login(access_token))
            {
                string json = JsonConvert.SerializeObject(login);
                Util.hs.SaveINISetting("NEST", "login", json, Util.IFACE_NAME + ".ini"); 
            }
        }

        public bool isAccessNotNull()
        {
            if (access_token != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool retrieveAccess(string code)
        {
            var client = new RestClient("https://api.home.nest.com/oauth2/access_token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "code=" + code + "&client_id=" + client_id + "&client_secret=" + client_secret + "&grant_type=authorization_code", ParameterType.RequestBody);

            try
            {
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);
                try
                {
                    using (var login = JsonConvert.DeserializeObject<Login>(response.Content))
                    {
                        if (login.access_token != null)
                        {
                            access_token = login.access_token;

                            saveLogin();
                            return true;
                        }
                        else
                        {
                            return false;
                        } 
                    }
                }
                catch (Exception e)
                {
                    Util.Log( e.ToString(), Util.LogType.LOG_TYPE_ERROR);
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

        public void connectNestData()
        {
            var client = new RestClient("https://developer-api.nest.com/");
            client.FollowRedirects = false;
            var request = getRequestGetOrPut(Method.GET, null, true);
            IRestResponse initial_response = client.Execute(request);

            if (initial_response.StatusCode == HttpStatusCode.RedirectKeepVerb) // 307
            {
                string newPath = "";
                foreach (var item in initial_response.Headers)
                {
                    if (item.Name.Equals("Location"))
                    {
                        newPath = item.Value.ToString();
                    }
                }
                //System.IO.File.WriteAllText(@"Data/hspi_nestsiid/setapi.txt", "1");
                var firebaseClient = new FirebaseClient(newPath, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(access_token) });
                //System.IO.File.WriteAllText(@"Data/hspi_nestsiid/setapi.txt", "2");
                var observable = firebaseClient.Child("devices").AsObservable<Devices>().Subscribe(d => Util.Find_Create_Devices(d.Object));
                var observable2 = firebaseClient.Child("structures").AsObservable<Dictionary<string, Structures>>().Subscribe(d => Util.Find_Create_Structures(d.Object));
                HSPI.observeRunning = true;           
            }
        }

        public NestData getNestData()
        {
            var client = new RestClient("https://developer-api.nest.com/");
            client.FollowRedirects = false;

            var request = getRequestGetOrPut(Method.GET, null, false);
            IRestResponse initial_response = client.Execute(request);

            if (initial_response.StatusCode == HttpStatusCode.RedirectKeepVerb) // 307
            {

                string newPath = "";
                foreach (var item in initial_response.Headers)
                {
                    if (item.Name.Equals("Location"))
                    {
                        newPath = item.Value.ToString();
                    }
                }
                //Console.WriteLine(newPath);
                System.IO.File.WriteAllText(@"Data/hspi_nestsiid/uri.txt", newPath);
                client.BaseUrl = new Uri(newPath);
                initial_response = client.Execute(request);


            }
            return JsonConvert.DeserializeObject<NestData>(initial_response.Content);
        }

        public void setApiJson(string json, string deviceType, string id)
        {
            var client = new RestClient("https://developer-api.nest.com/" + deviceType + "/" + id); // deviceType: "cameras", "thermometers"
            client.FollowRedirects = false;
            var request = getRequestGetOrPut(Method.PUT, json, false);

            IRestResponse initial_response = client.Execute(request);
            
            if (initial_response.StatusCode == HttpStatusCode.RedirectKeepVerb) // 307
            {
                string newPath = "";
                foreach (var item in initial_response.Headers)
                {
                    if (item.Name.Equals("Location"))
                    {
                        newPath = item.Value.ToString();
                        break;
                    }
                }
                client.BaseUrl = new System.Uri(newPath);
                initial_response = client.Execute(request);
                //System.IO.File.WriteAllText(@"Data/hspi_nestsiid/setapi2.txt", initial_response.Content);
                if(initial_response.StatusCode != HttpStatusCode.OK)
                {
                    Util.Log(initial_response.Content, Util.LogType.LOG_TYPE_WARNING);
                }
            }
        }
        // Disposable Interface
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NestConnection()
        {
            Debug.Assert(Disposed, "WARNING: Object finalized without being disposed!");
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                }

                DisposeUnmanagedResources();
                Disposed = true;
            }
        }

        protected virtual void DisposeManagedResources() { }
        protected virtual void DisposeUnmanagedResources() { }
    }
}
