using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace TrueCraftPushRestart
{
    public class GitHubHookListener
    {
        public event EventHandler PushReceived;

        private HttpListener _listener;

        public string Host { get; protected set; }

        public string SecureKey { get; set; }

        public GitHubHookListener(string host, string secureKey)
        {
            Host = host;
            SecureKey = secureKey;
        }

        public void Start()
        {
            if (_listener != null && _listener.IsListening)
                throw new Exception("Already running!");

            Thread runThread = new Thread(Run);
            runThread.IsBackground = true;

            runThread.Start();
        }

        protected void Run()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(Host);
            _listener.Start();

            _listener.BeginGetContext(GetContext, null);
        }

        private void GetContext(IAsyncResult iar)
        {
            var ctx = _listener.EndGetContext(iar);
            var request = ctx.Request;

            string evt = request.Headers["X-Github-Event"];
            string sig = request.Headers["X-Hub-Signature"];
            string del = request.Headers["X-Github-Delivery"];

            if (string.IsNullOrEmpty(evt) || string.IsNullOrEmpty(sig) || string.IsNullOrEmpty(del))
            {
                if (_listener.IsListening)
                    _listener.BeginGetContext(GetContext, null);

                return;
            }

            string body;
            using (StreamReader reader = new StreamReader(request.InputStream))
            {
                body = reader.ReadToEnd();
            }

            if (VerifySignature(SecureKey, body, Encoding.ASCII.GetBytes(sig.Replace("sha1=", ""))))
            {
                if (evt == "push")
                {
                    if (PushReceived != null)
                        PushReceived(this, EventArgs.Empty);
                }
                else
                {
                    Console.WriteLine("Received " + evt + " event. Ignoring.");
                }
            }
            else
            {
                Console.WriteLine("Invalid GitHub signature!");
            }

            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            ctx.Response.OutputStream.Dispose();

            if (_listener.IsListening)
                _listener.BeginGetContext(GetContext, null);
        }

        protected bool VerifySignature(string key, string body, byte[] hash)
        {
            using (HMACSHA1 hmac = new HMACSHA1(Encoding.ASCII.GetBytes(key)))
            {
                byte[] data = Encoding.ASCII.GetBytes(body);

                string hex = hmac.ComputeHash(data).Aggregate("", (s, e) => s + string.Format("{0:x2}", e), s => s);
                byte[] hexData = Encoding.ASCII.GetBytes(hex);

                if (hash.Length != hexData.Length)
                    return false;

                for (int i = 0; i < hash.Length; i++)
                {
                    if (hash[i] != hexData[i])
                        return false;
                }

                return true;
            }
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }
}
