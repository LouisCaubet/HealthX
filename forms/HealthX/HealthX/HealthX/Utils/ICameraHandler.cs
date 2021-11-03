using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthX.Utils {

    public interface ICameraHandler {

        void TakePicture();
        void SetCompletionAction(Action<int, byte[]> action);

    }

}
