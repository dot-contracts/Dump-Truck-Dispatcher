using System.Collections.Generic;

namespace DispatcherWeb.Dto
{
    public class IdListInput
    {
        public IdListInput()
        {
        }

        public IdListInput(List<int> ids)
        {
            Ids = ids;
        }

        public List<int> Ids { get; set; }
    }
}
