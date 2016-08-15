using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AllSectorsVirtV5.DAL;
using AllSectorsVirtV5.Models;

namespace AllSectorsVirtV5.Controllers
{
    public class FilesController : Controller
    {
        private AllSectorsDBContext db = new AllSectorsDBContext();
        //
        // GET: /File/
        public ActionResult Index(int id)
        {
            var fileToRetrieve = db.Files.Find(id);
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }
    }
}
