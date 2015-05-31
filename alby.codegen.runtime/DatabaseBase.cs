using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace alby.codegen.runtime
{
	public class DatabaseBase<T> where T: DatabaseBaseSingletonHelper 
	{
		protected static string __nameˡ			= "" ;
		protected static string __defaultNameˡ	= "" ;

		public static void Initˡ( string name ) 
		{
			__nameˡ = name ;
			__defaultNameˡ = name ;
		}

		public string DefaultNameˡ
		{
			get
			{
				return __defaultNameˡ ;
			}
		}

		public string Nameˡ
		{
			get
			{
				return __nameˡ ;
			}
			set
			{
				__nameˡ = value ;
			}
		}

		public void ResetToDefaultDatabase()
		{
			this.Nameˡ = this.DefaultNameˡ ;
		}

	} // end class
}
