using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncAwait
{
    //http://blog.stephencleary.com/2012/07/dont-block-on-async-code.html

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static async Task<string> GetJsonAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var jsonString = await client.GetStringAsync(uri);
                return jsonString;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            
            var jsonTask = GetJsonAsync(new System.Uri("https://www.google.co.in/"));
            //this will hang system.
            //richTextBox1.Text = jsonTask.Result; 
            Text = await jsonTask;
        }

        //In our case context is UI thread but a context can also be a ASP.NET request context for Web App
        //refer blog for more details.

        /* WHY DEADLOCK?
        So this is what happens, starting with the top-level method (Button1_Click for UI / MyController.Get 
        for ASP.NET):

        The top-level method calls GetJsonAsync (within the UI/ASP.NET context).
        
        GetJsonAsync starts the REST request by calling HttpClient.GetStringAsync (still within the context).
        
        GetStringAsync returns an uncompleted Task, indicating the REST request is not complete.
        
        GetJsonAsync awaits the Task returned by GetStringAsync. The context is captured and will be used to 
        continue running the GetJsonAsync method later. GetJsonAsync returns an uncompleted Task, 
        indicating that the GetJsonAsync method is not complete.
        
        The top-level method synchronously blocks on the Task returned by GetJsonAsync. This blocks the context 
        thread.
        
        ... Eventually, the REST request will complete. This completes the Task that was returned by GetStringAsync.
        The continuation for GetJsonAsync is now ready to run, and it waits for the context to be available so it 
        can execute in the context.
        
        Deadlock. The top-level method is blocking the context thread, waiting for GetJsonAsync to complete, 
        and GetJsonAsync is waiting for the context to be free so it can complete.
        For the UI example, the “context” is the UI context; for the ASP.NET example, the “context” is 
        the ASP.NET request context. This type of deadlock can be caused for either “context”. 
        */

        //Solution
        /*
         * 1. In your “library” async methods, use ConfigureAwait(false) wherever possible.
         * 2. Don’t block on Tasks; use async all the way down.
        */

    }
}
