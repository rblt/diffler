using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Machine.Specifications;
using rblt.Tools;

namespace rblt.Tests.Tools
{
    [Subject("Tools.Diff")]
    public class When_diffing_equal_instances : DiffTestsBase
    {
        Establish context = () =>
        {
            SetupMocks();
        };


        Because of = () => Result = Diff.Them(TestInstance1, TestInstance2);


        It should_not_contain_any_differences = () => Result.ShouldBeNull();


        static IDictionary<string, object> Result;
    }
}
