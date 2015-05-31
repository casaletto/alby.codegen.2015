using System;
using System.Collections.Generic;
using System.Text;

// 1	= 1 thread only (old style) 
// n	= use n threads, up to max
// anything else = use maximum available threads (default)  

namespace alby.codegen.generator
{
	public class Settings
	{
		protected int _threads = 0 ;

		protected string GetConfigSetting( string setting )
		{
			return System.Configuration.ConfigurationManager.AppSettings[ setting ] ;
		}         		

		public Settings()
		{
			int maxThreads = System.Environment.ProcessorCount ;

			string multiThreading = this.GetConfigSetting( "MultiThreading" ) ?? "" ;

			if ( int.TryParse( multiThreading, out _threads ) ) // an int 
			{
				_threads = Math.Min( _threads, maxThreads ) ;
				_threads = Math.Max( _threads, 1 ) ;				
			}
			else
				_threads = maxThreads ;
		}

		public int Threads
		{
			get
			{
				return _threads ;
			}
		}

		public bool UseMultipleThreads
		{
			get
			{
				return _threads > 1 ;
			}
		}

	}


}
