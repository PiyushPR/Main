using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using System.Net;
using System.Net.Security;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //string siteURL = "https://test-ap.insidenovelis.com/sites/AP/NovelisAwardsofExcellence";
            //string domain = "Novelis"; string username = "AgilePoint_Qa"; string password = "Workflow$1";

            //ClientContext context = new ClientContext(siteURL);
            ////SecureString securepass = new SecureString();
            ////foreach (char c in password.ToCharArray()) securepass.AppendChar(c);
            ////context.Credentials = new SharePointOnlineCredentials(username, securepass);

            //context.Credentials = new NetworkCredential(username, password, domain);
            //string listName = "NominationSupportingMaterials2018";
            ////DeleteListItems(context, listName);

            //string srcLibrary = "NominationSupportingMaterials";
            //string destLibrary = "NominationSupportingMaterials2018";
            //string srcUrl = "https://test-ap.insidenovelis.com/sites/AP/NovelisAwardsofExcellence";
            //string destUrl = "https://test-ap.insidenovelis.com/sites/AP/NovelisAwardsofExcellence/Record 2018";
            //CopyDocLibRecursively(srcLibrary, destLibrary, srcUrl, destUrl, domain, username, password);

            Console.ReadLine();
        }

        static void DeleteListItems(ClientContext context, string listName)
        {
            List list = context.Web.Lists.GetByTitle(listName);
            CamlQuery query = new CamlQuery();
            query.ViewXml = "<View/>";
            ListItemCollection items = list.GetItems(query);
            context.Load(list);
            context.Load(items);
            //Uncommnet if SSL enabled site
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            context.ExecuteQuery();

            var totalListItems = 1;
            foreach (ListItem item in items.ToList())
            {
                if (totalListItems != 100)
                {
                    item.DeleteObject();
                }
                else break;
                totalListItems++;
            }

            context.ExecuteQuery();
            Console.WriteLine("Remaining Items : {0}", items.Count);
        }

        static void CopyDocLibRecursively(string srcLibrary, string destLibrary, string srcUrl, string destUrl, string domain, string username, string password)
        {
            ClientContext srcContext = new ClientContext(srcUrl);
            srcContext.Credentials = new NetworkCredential(username, password, domain);

            ClientContext destContext = new ClientContext(destUrl);
            destContext.Credentials = new NetworkCredential(username, password, domain);

            var srcList = srcContext.Web.Lists.GetByTitle(srcLibrary);
            var sqry = CamlQuery.CreateAllItemsQuery();
            //qry.FolderServerRelativeUrl = string.Format("/{0}", srcUrl);
            var srcItems = srcList.GetItems(sqry);
            srcContext.Load(srcItems, icol => icol.Include(i => i.FileSystemObjectType, i => i["FileRef"], i => i.File));
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            srcContext.ExecuteQuery();
            string srcRelativeUrl = "/sites/AP/NovelisAwardsofExcellence/Excellence Award Project Supporting Material";

            Web destWeb = destContext.Web;
            destContext.Load(destWeb);
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            destContext.ExecuteQuery();

            var counter = 0;
            foreach (var item in srcItems)
            {
                counter++;
                //Console.WriteLine(item.FileSystemObjectType + "||" + item.FieldValues.Values.ToList()[0]);
                switch (item.FileSystemObjectType)
                {
                    //case FileSystemObjectType.Folder:
                    //var folderName = item["FileRef"].ToString().Substring(item["FileRef"].ToString().LastIndexOf('/') + 1);
                    //CreateFolder(destContext.Web, destList, folderName);
                    //Console.WriteLine("[" + counter + "] " + folderName + " is created.");
                    //break;
                    case FileSystemObjectType.File:
                        string fileName = item["FileRef"].ToString();
                        string[] fileNames = fileName.Split(new string[] { srcLibrary }, StringSplitOptions.None);
                        fileName = fileNames[fileNames.Count() - 1];

                        File file = item.File;
                        srcContext.Load(file);
                        srcContext.ExecuteQuery();
                        string location = destWeb.ServerRelativeUrl.TrimEnd('/') + file.ServerRelativeUrl.Replace(srcRelativeUrl, "").Replace(file.Name, "").TrimEnd('/');
                        FileInformation fileInfo = File.OpenBinaryDirect(srcContext, file.ServerRelativeUrl);
                        File.SaveBinaryDirect(destContext, location, fileInfo.Stream, true);
                        break;
                }
            }
            Console.Write(counter);
        }
        private static void CreateFolder(Web web, List destList, string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) throw new ArgumentNullException("Folder Url could not be empty");

            ListItemCreationInformation newItemInfo = new ListItemCreationInformation();
            newItemInfo.UnderlyingObjectType = FileSystemObjectType.Folder;
            newItemInfo.LeafName = folderName;
            ListItem newListItem = destList.AddItem(newItemInfo);
            newListItem["Title"] = folderName;
            newListItem.Update();
            web.Context.ExecuteQuery();
        }
    }
}
