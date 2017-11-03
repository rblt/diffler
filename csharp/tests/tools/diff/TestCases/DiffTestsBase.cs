using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rblt.Tools;

namespace rblt.Tests.Tools
{
    public class DiffTestsBase
    {
        public class Complex
        {
            public int ID { get; set; }

            public string StringProp { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;

                Complex other = obj as Complex;
                if (other != null)
                    return this.ID == other.ID;

                return false;
            }

            public override int GetHashCode()
            {
                return this.ID.GetHashCode();
            }
        }

        public class TestClass
        {
            public int PropInt { get; set; }

            public DateTime PropDate { get; set; }

            public string PropString { get; set; }

            public object PropObject { get; set; }

            public Complex PropComplex { get; set; }

            public int[] PropArray { get; set; }

            public Complex[] PropComplexArray { get; set; }

            [DiffIgnore]
            public object PropIgnore { get; set; }

            [DiffIgnoreOrder]
            public int[] PropArray_IgnoreOrder { get; set; }

            [DiffIgnoreOrder]
            public Complex[] PropComplexArray_IgnoreOrder { get; set; }
        }

        public static void SetupMocks()
        {
            var oObj = new object();
            var oIgnore = new object();
            
            TestInstance1 = new TestClass()
            {
                PropInt = 1,
                PropDate = DateTime.MinValue,
                PropString = "TestInstance",
                PropObject = oObj,
                PropComplex = new Complex()
                {
                    ID = 100,
                    StringProp = "ComplexProp"
                },
                PropArray = new int[] { 1, 2, 3 },
                PropComplexArray = new Complex[] {
                    new Complex() { ID = 1 },
                    new Complex() { ID = 2 },
                    new Complex() { ID = 3 }
                },
                PropIgnore = oIgnore,
                PropArray_IgnoreOrder = new int[] { 3, 2, 1 },
                PropComplexArray_IgnoreOrder = new Complex[] {
                    new Complex() { ID = 3 },
                    new Complex() { ID = 2 },
                    new Complex() { ID = 1 }
                }
            };

            TestInstance2 = new TestClass()
            {
                PropInt = 1,
                PropDate = DateTime.MinValue,
                PropString = "TestInstance",
                PropObject = oObj,
                PropComplex = new Complex()
                {
                    ID = 100,
                    StringProp = "ComplexProp"
                },
                PropArray = new int[] { 1, 2, 3 },
                PropComplexArray = new Complex[] {
                    new Complex() { ID = 1 },
                    new Complex() { ID = 2 },
                    new Complex() { ID = 3 }
                },
                PropIgnore = oIgnore,
                PropArray_IgnoreOrder = new int[] { 2, 1, 3 },
                PropComplexArray_IgnoreOrder = new Complex[] {
                    new Complex() { ID = 1 },
                    new Complex() { ID = 3 },
                    new Complex() { ID = 2 }
                }
            };
        }

        public static TestClass TestInstance1;
        public static TestClass TestInstance2;
    }
}
