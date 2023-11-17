namespace Mosha.SimpleDecorator.Test.Unit;

    public class Tests
    {
        [Test]
        public void TestProxyGenerator()
        {
            Test T = new Test();

            ITest proxy = new ProxyGenerator().Generate<ITest, TestInseptor>(T);

            TestResult testResult = new TestResult();

            int result = proxy.Sum(1, 2, testResult);

            Assert.AreEqual(result, 3);

            Assert.True(testResult.IsSatisfied());

            Assert.Pass();
        }
    }

    #region Test Materials

    public class Test
    {
        public int Sum(int a, int b, TestResult Tresult)
        {
            Tresult.RunInTarget();
            return a + b;
        }
    }

    public interface ITest
    {
        public int Sum(int a, int b, TestResult Tresult);
    }

    public class TestInseptor : IInseptor
    {
        public object target { get; set; }

        public object Insept(Invoker invoker)
        {
            var Tresult = invoker.GetParameter<TestResult>(2);

            Tresult.RunBefore();

            var result = invoker.Run(target);

            Tresult.RunAfter();

            return result;
        }
    }


    public class TestResult
    {
        public bool Before { get; private set; }
        public bool InTarget { get; private set; }
        public bool After { get; private set; }
        public bool StateValid { get; private set; } = true;


        public void RunBefore() => Before = true;
        public void RunInTarget()
        {
            InTarget = true;
            if (!Before) StateValid = false;
        }
        public void RunAfter()
        {
            After = true;
            if (!Before || !InTarget) StateValid = false;
        }

        public bool IsSatisfied()
        {
            return Before && InTarget && After && StateValid;
        }
    }
    #endregion

