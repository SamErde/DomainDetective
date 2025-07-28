namespace DomainDetective.Tests
{
    public class TestPortHelper
    {
        [Fact]
        public async Task ReleasePortFromDifferentThreadDoesNotThrow()
        {
            var port = PortHelper.GetFreePort();
            await Task.Run(() => PortHelper.ReleasePort(port));
            var port2 = PortHelper.GetFreePort();
            PortHelper.ReleasePort(port2);
        }
    }
}
