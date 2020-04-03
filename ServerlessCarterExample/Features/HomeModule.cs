using System;
using System.Threading.Tasks;
using Carter;
using Microsoft.AspNetCore.Http;

namespace ServerlessCarterExample.Features
{


    public class HomeModule : CarterModule
    {
        public HomeModule()
        {
            this.Get("/", (req, res) =>
            {
                res.StatusCode = 200;
                return res.WriteAsync("Serverless Carter Example");
            });
        }
    }
}
