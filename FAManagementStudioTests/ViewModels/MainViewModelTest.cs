using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FAManagementStudio.ViewModels.Tests
{
    [TestClass]
    public class MainViewModelTest
    {

        [TestMethod]
        public void CreateSelectStatementTest()
        {
            var vm = new MainViewModel();
            var testCase = new[] {
                new { Input = new { TableName = "TEST", Colums = new[] { "A" } },
                      Answer = "select A from TEST" },
                new { Input = new { TableName = "TEST", Colums = new[] { "A", "B" } },
                      Answer = "select A, B from TEST" },
                new { Input = new { TableName = "TEST", Colums = new[] { "A", "Index" } },
                      Answer = "select A, 'Index' from TEST" }
            };
            foreach (var test in testCase)
            {
                var result = (string)vm.AsDynamic().CreateSelectStatement(test.Input.TableName, test.Input.Colums);
                result.Is(test.Answer);
            }
        }

        [TestMethod]
        public void CreateInsertStatementTest()
        {
            var vm = new MainViewModel();
            var testCase = new[] {
                new { Input = new { TableName = "TEST", Colums = new[] { "A" } },
                      Answer = "insert into TEST (A) values (@a)" },
                new { Input = new { TableName = "TEST", Colums = new[] { "A", "B" } },
                      Answer = "insert into TEST (A, B) values (@a, @b)" },
                new { Input = new { TableName = "TEST", Colums = new[] { "A", "Index" } },
                      Answer = "insert into TEST (A, 'Index') values (@a, @index)" }
            };
            foreach (var test in testCase)
            {
                var result = (string)vm.AsDynamic().CreateInsertStatement(test.Input.TableName, test.Input.Colums);
                result.Is(test.Answer);
            }
        }

        [TestMethod]
        public void CreateUpdateStatementTest()
        {
            var vm = new MainViewModel();
            var testCase = new[] {
                new { Input = new { TableName = "TEST", Colums = new[] { "A" } },
                      Answer = "update TEST set A = @a" },
                new { Input = new { TableName = "TEST", Colums = new[] { "A", "B" } },
                      Answer = "update TEST set A = @a, B = @b" },
                new { Input = new { TableName = "TEST", Colums = new[] { "A", "Index" } },
                      Answer = "update TEST set A = @a, 'Index' = @index" }
            };
            foreach (var test in testCase)
            {
                var result = (string)vm.AsDynamic().CreateUpdateStatement(test.Input.TableName, test.Input.Colums);
                result.Is(test.Answer);
            }
        }
    }
}
