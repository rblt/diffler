using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Machine.Specifications;
using rblt.Tools;

namespace rblt.Tests.Tools
{
    [Subject("Tools.Diff")]
    public class When_diffing_unequal_instances_3 : DiffTestsBase
    {
        Establish context = () =>
        {
            SetupMocks();

            PropInt = 2;
            PropDate = new DateTime(2015, 01, 01, 12, 00, 00);
            PropString = "OtherPropString";

            TestInstance2.PropInt = PropInt;
            TestInstance2.PropDate = PropDate;
            TestInstance2.PropString = PropString;
        };


        Because of = () => Result = Diff.Them(TestInstance1, TestInstance2);


        It should_contain_3_differences = () => Result.ShouldContainOnly(
            new KeyValuePair<string, object>("PropInt", PropInt),
            new KeyValuePair<string, object>("PropDate", PropDate),
            new KeyValuePair<string, object>("PropString", PropString)
        );

        static int PropInt;
        static DateTime PropDate;
        static string PropString;
        static IDictionary<string, object> Result;
    }
}
