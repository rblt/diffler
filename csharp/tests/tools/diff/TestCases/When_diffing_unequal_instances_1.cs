using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Machine.Specifications;
using rblt.Tools;

namespace rblt.Tests.Tools
{
    [Subject("Tools.Diff")]
    public class When_diffing_unequal_instances_1 : DiffTestsBase
    {
        Establish context = () =>
        {
            SetupMocks();

            PropInt = 2;

            TestInstance2.PropInt = PropInt;
        };


        Because of = () => Result = Diff.Them(TestInstance1, TestInstance2);


        It should_contain_only_1_difference = () => Result.ShouldContainOnly(new KeyValuePair<string, object>("PropInt", PropInt));


        static int PropInt;
        static IDictionary<string, object> Result;
    }
}
