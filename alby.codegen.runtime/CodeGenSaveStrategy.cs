using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace alby.codegen.runtime
{
	public enum CodeGenSaveStrategy
	{
		Normal = 0,
		ForceSaveTryUpdateFirstThenInsert,
		ForceSaveTryInsertFirstThenUpdate
	}
}
