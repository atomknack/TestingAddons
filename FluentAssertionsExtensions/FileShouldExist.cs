using FluentAssertions;
using System.Collections;
using System.Collections.Generic;

public static class FluentAssertionExtensions
{
    public static FileToTestAssertion Should(this FileToTest file) => new FileToTestAssertion { File = file };
    public class FileToTestAssertion
    {
        public FileToTest File;
        public void NotExist(string because = "", params object[] reasonArgs)
            => System.IO.File.Exists(File.Path).Should().BeFalse(because, reasonArgs);
        public void Exist(string because = "", params object[] reasonArgs)
            => System.IO.File.Exists(File.Path).Should().BeTrue(because, reasonArgs);
    }
    public static FileToTest FilePathToTest(this string fName) => new FileToTest { Path = fName };
    public class FileToTest { public string Path; }
}

