using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WcfService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        string GetData(string value);

        [OperationContract]
        LargeDataStructures GetLargeData();
    }

    [CollectionDataContract]
    public class LargeDataStructures : List<LargeDataStructure>
    {
    }

    [DataContract]
    public class LargeDataStructure
    {
        [DataMember]
        public string FirstName;

        [DataMember]
        public string LastName;

        [DataMember]
        public int Age;

        [DataMember]
        public string Country;

        [DataMember]
        public string City;

        [DataMember]
        public string Description;
    }
}
