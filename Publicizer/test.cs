using System;
using System.Collections.Generic;
using System.Text;

namespace Publicizer.Tests
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
    class Test
    {

        private int field;

        public int Publicproperty { get; private set; }

        private int Property { get; set; }


        private int Method()
        {
            field = 2;
            Publicproperty = Property + field;
            return 1;
        }

        private class NestedTest
        {
            private int innerField;

            NestedTest()
            {
                innerField = 3;
                if(innerField != 3)
                {
                    Console.WriteLine("Look at you go!");
                }
            }
        }
    }

#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
}
