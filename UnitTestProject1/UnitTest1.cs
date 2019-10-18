using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApp1;

namespace UnitTestProject1 {
    [TestClass]
    public class UnitTests {
        [TestMethod]
        public void TestMethod1() {
            const int CONSOLE_SHIFT = JobLogger.CONSOLE_SHIFT; // == 0;
            const int FILE_SHIFT = JobLogger.FILE_SHIFT; // == 1;
            const int DB_SHIFT = JobLogger.DB_SHIFT; // == 2;
            const int INFO_SHIFT = 3;
            const int WARNING_SHIFT = 4;
            const int ERROR_SHIFT = 5;
            //Here the mask will represent the status of boolean flags passed to JobLogger's constructor;
            //The following order of bits is used:
            //zero bit -> corresponds to _logToConsole
            //1-st bit -> corresponds to _logToFile
            //2-nd bit -> corresponds to _logToDatabase
            //3-rd bit -> corresponds to _logInfo
            //4-th bit -> corresponds to _logWarning
            //5-th bit -> corresponds to _logError
            for (int mask = 0; mask < (1 << 6); mask++) {
                JobLogger TempLogger = new JobLogger((mask & (1 << FILE_SHIFT)) != 0, (mask & (1 << CONSOLE_SHIFT)) != 0,
                    (mask & (1 << DB_SHIFT)) != 0, (mask & (1 << INFO_SHIFT)) != 0, (mask & (1 << WARNING_SHIFT)) != 0,
                    (mask & (1 << ERROR_SHIFT)) != 0);
                for(int msg_type = 0; msg_type < 3; msg_type++) {
                    int buf = TempLogger.LogMessage("It is just a test message", msg_type);
                    //If JobLogger's flag for corresponded msg_type is clear, than JobLogger should output nothing
                    //Let's test this case
                    if(msg_type == JobLogger.INFO && (mask & (1 << INFO_SHIFT)) == 0
                        || msg_type == JobLogger.WARNING && (mask & (1 << WARNING_SHIFT)) == 0
                        || msg_type == JobLogger.ERROR && (mask & (1 << ERROR_SHIFT)) == 0) {
                        Assert.AreEqual(0, buf);
                    }
                    else {
                        //Ok, our JobLogger SHOULD process the message and output something, according to
                        //_logToConsole, _logToFile and _logToDatabase flags
                        //Let's check if corresponding outputs took place
                        int expected = (mask & (1 << CONSOLE_SHIFT)) + (mask & (1 << FILE_SHIFT))
                            + (mask & (1 << DB_SHIFT));
                        Assert.AreEqual(expected, buf);
                    }
                }
            }
        }
    }
}
