using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClyngMobile
{
    public interface CMClientListener
    {
        void onError(Exception error);
        void onSuccess();
    }
}
