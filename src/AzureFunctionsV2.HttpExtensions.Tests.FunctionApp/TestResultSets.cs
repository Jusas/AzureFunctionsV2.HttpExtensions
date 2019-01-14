using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsV2.HttpExtensions.Tests.FunctionApp
{
    public enum TestEnum
    {
        Zero,
        One,
        Two,
        Three
    }

    public class HeaderTestResultSet
    {
        public string StringParam { get; set; }
        public int IntParam { get; set; }
        public TestEnum EnumParam { get; set; }
    }

    public class QueryTestResultSet
    {
        public string StringParam { get; set; }
        public int IntParam { get; set; }
        public TestEnum EnumParam { get; set; }
    }

    public class QueryArrayTestResultSet
    {
        public string[] StringArray { get; set; }
        public int[] IntArray { get; set; }
        public TestEnum[] EnumArray { get; set; }
    }

    public class QueryListTestResultSet
    {
        public List<string> StringList { get; set; }
        public List<int> IntList { get; set; }
        public List<TestEnum> EnumList { get; set; }
    }
}
