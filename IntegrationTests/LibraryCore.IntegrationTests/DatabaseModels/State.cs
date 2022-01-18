using System;
using System.Collections.Generic;

namespace LibraryCore.IntegrationTests.DatabaseModels
{
    public partial class State
    {
        public Guid TestId { get; set; }
        public int StateId { get; set; }
        public string Description { get; set; } = null!;
    }
}
