using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Machine.Specifications;
using rblt.Tools;

namespace rblt.Tests.Tools
{
    [Subject("Tools.Diff")]
    public class When_diffing_unequal_instances_6 : DiffTestsBase
    {
        Establish context = () =>
        {
            SetupMocks();

            PropIgnore = new object();

            TestInstance2.PropIgnore = PropIgnore;
        };


        Because of = () => Result = Diff.Them(TestInstance1, TestInstance2);


        It should_not_contain_ignored_difference = () => Result.ShouldBeNull();


        static object PropIgnore;
        static IDictionary<string, object> Result;
    }
}
