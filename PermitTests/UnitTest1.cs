using Xunit;
using System.Collections.Generic;
using Moq;
using Permit;
using Permit.Models;

namespace Permit.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }

        [Fact]
        public async void TestPermitCheck()
        {
            Dictionary<string, string> openWith = new Dictionary<string, string>();
            User myUser = new User("test@email.com");
            var testUser = (IUserKey) (new Dictionary<string, string> { { "key" , "test" } });
            ResourceInput resource = new ResourceInput("Document");

            // Add some elements to the dictionary. There are no
            // duplicate keys, but some of the values are duplicates.
            openWith.Add("txt", "notepad.exe");

            //var enforcer = new Enforcer("testurl");
            //Assert.True(await enforcer.Check("user", "action", "resource", openWith));
            //Assert.True(await enforcer.Check(testUser, "action", resource, openWith));
        }

        [Fact]
        public void TestPermitConfig()
        {
            string testToken = "testToken";
            Config config = new Config(testToken);
            Assert.True(config.Token == testToken);
            Assert.True(config.Pdp == Config.DEFAULT_PDP_URL);
        }

        [Fact]
        public async void TestPermitClient()
        {
            //setup http server on port 7000
            //assert got request
            //using (new MockServer(7000, "/allowed", (req, rsp, prm) => "Result Body"))
            //{
                string testToken = "testToken";
                string testUser = "testUser";
                //var testUserKey = (IUserKey)( { { "key", "test" } });
                Mock<IUserKey> testUserKey = new Mock<IUserKey>();
                testUserKey.Setup(testUserKey => testUserKey.key).Returns("test");
            
                string testAction = "testAction";
                string testResource = "testResource";
                Client permitClient = new Client(testToken);
                Assert.True(await permitClient.enforcer.Check(testUser, testAction, testResource));
                Assert.True(await permitClient.enforcer.Check(testUserKey.Object, testAction, new ResourceInput(testResource)));
            //}

        }
    }
}

