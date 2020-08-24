using System;
using Xunit;

namespace svr_framework.tests
{
    public class TestAccountContext
    {
        private AccountContext _db;
        private Account _ac;

        public TestAccountContext()
        {
            Console.WriteLine(System.Environment.CurrentDirectory);
            _db = new AccountContext("../../../../svr_framework/bin/account.db");
            _ac = new Account(){ Id = "yangxun",Pw = "123456"};
        }

        [Fact]
        public void Reg()
        {
           _db.Register(_ac);
            Assert.Throws<System.ArgumentException>(()=>_db.Register(_ac));
            _db.Register(new Account(){ Id="杨循",Pw="壹贰叁肆伍陆染捌玖拾"});
        }

        [Fact]
        public void Login()
        {
            Assert.True(_db.IsExists(_ac));
            Assert.True(_db.Remove(_ac));
            Assert.False(_db.IsExists(_ac));
            Assert.False(_db.Remove(_ac));
        }
    }
}
