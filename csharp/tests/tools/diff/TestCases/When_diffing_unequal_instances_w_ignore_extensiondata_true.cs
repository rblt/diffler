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
    public class When_diffing_unequal_instances_w_ignore_extensiondata_true : DiffTestsBase
    {
        Establish context = () =>
        {
            SetupMocks();

            ExtensionData = new object();

            TestInstance2.ExtensionData = ExtensionData;

            Diff.IgnoreExtensionDataProperties = true;
        };


        Because of = () => Result = Diff.Them(TestInstance1, TestInstance2);


        It should_not_contain_any_differences = () => Result.ShouldBeNull();

        static object ExtensionData;
        static IDictionary<string, object> Result;
    }
}
