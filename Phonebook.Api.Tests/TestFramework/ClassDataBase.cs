using System.Collections;
using System.Collections.Generic;

namespace Phonebook.Api.Tests.TestFramework
{
    internal abstract class ClassDataBase : IEnumerable<object[]>
    {
        protected abstract List<object?[]> Data { get;}

        public IEnumerator<object[]> GetEnumerator()
        { return Data.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }
    }
}
