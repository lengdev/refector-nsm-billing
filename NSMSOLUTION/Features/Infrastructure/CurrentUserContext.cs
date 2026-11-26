using MVC;
using System.Web;

namespace NSMSOLUTION.Features.Infrastructure
{
    public class CurrentUserContext
    {
        private readonly HTML _html;

        public CurrentUserContext()
        {
            _html = new HTML();
        }

        public string DomainID
        {
            get { return _html.DomainID; }
        }

        public string SystemID
        {
            get { return _html.SystemID; }
        }

        public string WarehouseID
        {
            get { return _html.WarehouseID; }
        }

        public string UserID
        {
            get { return _html.UserID; }
        }

        public string Username
        {
            get
            {
                return _html.UserFullName;
            }
        }
    }
}