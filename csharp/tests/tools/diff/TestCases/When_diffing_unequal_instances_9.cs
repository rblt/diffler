using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Machine.Specifications;
using rblt.Tools;

namespace rblt.Tests.Tools
{
    [Subject("Tools.Diff")]
    public class When_diffing_unequal_instances_9 : DiffTestsBase
    {
        Establish context = () =>
        {
            SetupMocks();

            PropInt = 2;
            PropDate = new DateTime(2015, 01, 01, 12, 00, 00);
            PropString = "OtherPropString";
            PropObject = new object();
            PropComplex = new Complex()
            {
                ID = 123,
                StringProp = "123"
            };
            PropArray =  new int[] { 3, 2, 1 };
            PropComplexArray = new Complex[] {
                new Complex() { ID = 3 },
                new Complex() { ID = 2 },
                new Complex() { ID = 1 }
            };
            PropArray_IgnoreOrder = new int[] { 4, 3, 2 };

            TestInstance2.PropInt = PropInt;
            TestInstance2.PropDate = PropDate;
            TestInstance2.PropString = "OtherPropString";
            TestInstance2.PropObject = PropObject;
            TestInstance2.PropComplex = PropComplex;
            TestInstance2.PropArray = PropArray;
            TestInstance2.PropComplexArray = PropComplexArray;
            TestInstance2.PropArray_IgnoreOrder = PropArray_IgnoreOrder;
        };


        Because of = () => Result = Diff.Them(TestInstance1, TestInstance2);


        It should_contain_8_differences = () => Result.ShouldContainOnly(
            new KeyValuePair<string, object>("PropInt", PropInt),
            new KeyValuePair<string, object>("PropDate", PropDate),
            new KeyValuePair<string, object>("PropString", PropString),
            new KeyValuePair<string, object>("PropObject", PropObject),
            new KeyValuePair<string, object>("PropComplex", PropComplex),
            new KeyValuePair<string, object>("PropArray", PropArray),
            new KeyValuePair<string, object>("PropComplexArray", PropComplexArray),
            new KeyValuePair<string, object>("PropArray_IgnoreOrder", PropArray_IgnoreOrder)
        );

        static int PropInt;
        static DateTime PropDate;
        static string PropString;
        static object PropObject;
        static Complex PropComplex;
        static int[] PropArray;
        static Complex[] PropComplexArray;
        static int[] PropArray_IgnoreOrder;
        static IDictionary<string, object> Result;
    }
}
