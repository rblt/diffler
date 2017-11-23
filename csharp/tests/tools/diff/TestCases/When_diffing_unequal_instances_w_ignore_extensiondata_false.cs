using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using rblt.Tools;

namespace rblt.Tests.Tools
{
    [Subject("Tools.Diff")]
    public class When_diffing_unequal_instances_w_ignore_extensiondata_false : DiffTestsBase
    {
        Establish context = () =>
        {
            SetupMocks();

            ExtensionData = new object();

            TestInstance2.ExtensionData = ExtensionData;

            Diff.IgnoreExtensionDataProperties = false;
        };


        Because of = () => Result = Diff.Them(TestInstance1, TestInstance2);


        It should_contain_only_1_difference = () => Result.ShouldContainOnly(new KeyValuePair<string, object>("ExtensionData", ExtensionData));

        static object ExtensionData;
        static IDictionary<string, object> Result;
    }
}
