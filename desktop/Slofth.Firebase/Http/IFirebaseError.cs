using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slofth.Firebase.Http
{
    interface IFirebaseError
    {
        Exception GetCorrespondingException();
    }
}
