
-------------------------------------------------------------------------------------------------------------------------

if ( OBJECT_ID( 'tempdb..#tables') is not null )
	drop table #tables

if ( OBJECT_ID( 'tempdb..#views') is not null )
	drop table #views

if ( OBJECT_ID( 'tempdb..#sp') is not null )
	drop table #sp

if ( OBJECT_ID( 'tempdb..#pk') is not null )
	drop table #pk
	
if ( OBJECT_ID( 'tempdb..#fk') is not null )
	drop table #fk

if ( OBJECT_ID( 'tempdb..#fk2pk') is not null )
	drop table #fk2pk

if ( OBJECT_ID( 'tempdb..#pk2fk') is not null )
	drop table #pk2fk

if ( OBJECT_ID( 'tempdb..#tscol') is not null )
	drop table #tscol

if ( OBJECT_ID( 'tempdb..#idcol') is not null )
	drop table #idcol

if ( OBJECT_ID( 'tempdb..#compcol') is not null )
	drop table #idcol

if ( OBJECT_ID( 'tempdb..#spparam') is not null )
	drop table #spparam

if ( OBJECT_ID( 'tempdb..#tabletype') is not null )
	drop table #tabletype

if ( OBJECT_ID( 'tempdb..#tabletypecol') is not null )
	drop table #tabletypecol

-------------------------------------------------------------------------------------------------------------------------
-- the tables -----------------------------------------------------------------------------------------------------------

select	 
	table_schema + '.' + table_name as TheTable 
into
	#tables
from	 
	information_schema.tables  
where 	 
	table_type = 'BASE TABLE'  
order by 1

-------------------------------------------------------------------------------------------------------------------------
-- the views -----------------------------------------------------------------------------------------------------------

select 
	table_schema + '.' + table_name as TheView 
into
	#views
from 
	information_schema.views 
order by 1			

-------------------------------------------------------------------------------------------------------------------------
-- the stored procedures ------------------------------------------------------------------------------------------------

select 
	specific_schema + '.' + specific_name as TheStoredProcedure
into
	#sp
from 
	information_schema.routines
where
	routine_type = 'PROCEDURE'
order by	
	1

-------------------------------------------------------------------------------------------------------------------------
-- primary keys	---------------------------------------------------------------------------------------------------------

select 
	OBJECT_SCHEMA_NAME( kc.parent_object_id ) + '.' + OBJECT_NAME( kc.parent_object_id ) as PkTable, 
	kc.type as PkConstraintType,
	name as PkName, 
	ic.index_column_id as PkColumnId,
	ic.column_id as PkTableColumnId, 		
	COL_NAME( kc.parent_object_id, ic.column_id ) as PkTableColumnName		
into 
	#pk
from	
	sys.key_constraints kc,  
	sys.index_columns ic
where	
	kc.parent_object_id = ic.object_id 
--and		
--	kc.type = 'PK'
and
	kc.unique_index_id = ic.index_id 	
order by 
	1, 2, 3, 4, 5, 6

-------------------------------------------------------------------------------------------------------------------------
-- foreign keys and their parents ---------------------------------------------------------------------------------------

select 
	OBJECT_SCHEMA_NAME( parent_object_id ) + '.' + OBJECT_NAME( parent_object_id ) as FkTable, 
	OBJECT_NAME( constraint_object_id ) as FkName, 
	constraint_column_id as FkColumnId, 
	parent_column_id as FkTableColumnId, 
	COL_NAME( parent_object_id, parent_column_id ) as FkTableColumnName,
	OBJECT_SCHEMA_NAME( referenced_object_id ) + '.' + OBJECT_NAME( referenced_object_id ) as PkTable, 
	referenced_column_id as PkTableColumnId, 
	COL_NAME( referenced_object_id, referenced_column_id ) as PkTableColumnName
into 
	#fk
from 
	sys.foreign_key_columns 
order by 
	1, 2, 3, 4, 6 ,7, 8

------------------------------------------------------------------------------------------------------------------------
-- foreign keys mapped tp primary keys ---------------------------------------------------------------------------------

select	
	#fk.*, 
	#pk.PkName, 
	#pk.PkColumnId,
	#pk.PkConstraintType
into
	#fk2pk
from	
	#fk, #pk
where	
	#fk.PkTable = #pk.PkTable
and		
	#fk.PkTableColumnId = #pk.PkTableColumnId
order by 
	1,2,3,4,5,6,7,8

-------------------------------------------------------------------------------------------------------------------------
-- primary key to foreign key map ---------------------------------------------------------------------------------------

select 
	PkTable,	
	PkName,	
	PkConstraintType,
	PkColumnId,
	PkTableColumnName,	
	PkTableColumnId,	
	FkName,	
	FkTable,	
	FkColumnId,	
	FkTableColumnId,	
	FkTableColumnName
into
	#pk2fk		
from 
	#fk2pk
order by 
	1,2,3,4,5,6,7,8,9,10,11

-------------------------------------------------------------------------------------------------------------------------
-- timestamp columns ----------------------------------------------------------------------------------------------------

select	 
	table_schema + '.' + table_name as TheTable, 
	column_name as TimestampColumn
into
	#tscol
from	 
	information_schema.columns 
where	
	data_type = 'TIMESTAMP' 
order by 1, 2

-------------------------------------------------------------------------------------------------------------------------
-- identity columns -----------------------------------------------------------------------------------------------------

select   
	OBJECT_SCHEMA_NAME( c.id ) + '.' + OBJECT_NAME( c.id ) as TheTable, 
	c.name as IdentityColumn
into
	#idcol
from
	sys.syscolumns c
where	
	columnproperty( c.id, c.name , 'IsIdentity' ) = 1 
order by 
	1, 2

-------------------------------------------------------------------------------------------------------------------------
-- computed columns -----------------------------------------------------------------------------------------------------

select   
	OBJECT_SCHEMA_NAME( c.id ) + '.' + OBJECT_NAME( c.id ) as TheTable, 
	c.name as ComputedColumn
into
	#compcol
from
	sys.syscolumns c
where	
	columnproperty( c.id, c.name , 'IsComputed' ) = 1 
order by 
	1, 2

-------------------------------------------------------------------------------------------------------------------------
-- stored procedure parameters ------------------------------------------------------------------------------------------

select 
	OBJECT_SCHEMA_NAME( p.object_id ) + '.' + OBJECT_NAME( p.object_id ) as TheStoredProcedure,
	p.parameter_id,
	p.name,
	coalesce( SCHEMA_NAME( tt.schema_id ) + '.', ''  ) + t.name as type,
	t.is_table_type,
	p.is_output,
	p.max_length,
	p.precision,
	p.scale,
	p.system_type_id,
	p.user_type_id,
	t2.name as type2,
	isp.character_maximum_length,
	isp.numeric_precision, 
	isp.numeric_scale
into
	#spparam
from
	sys.parameters p 
inner join	
	sys.types t
		on	
			p.user_type_id = t.user_type_id
left join
	sys.types t2	
		on
			p.system_type_id = t2.user_type_id
left join
	sys.table_types	tt		
		on 
			p.user_type_id = tt.user_type_id 	
left join
	INFORMATION_SCHEMA.PARAMETERS isp
		on	
			OBJECT_SCHEMA_NAME( p.object_id )	= isp.SPECIFIC_SCHEMA 
		and OBJECT_NAME( p.object_id )			= isp.SPECIFIC_NAME 
		and p.parameter_id						= isp.ORDINAL_POSITION					
where
	p.parameter_id >= 1
order by 
	1, 2

-------------------------------------------------------------------------------------------------------------------------
-- table types ----------------------------------------------------------------------------------------------------------

select distinct
	[type]
into
	#tabletype
from
	#spparam
where
	is_table_type = 'true'
order by
	1

-------------------------------------------------------------------------------------------------------------------------
-- table type columns ---------------------------------------------------------------------------------------------------

select 
	SCHEMA_NAME( tt.schema_id  ) + '.' + tt.name as TableType,
	c.name			as ColumnName,
	c.column_id		as ColumnOrder,
	ct.Name			as ColumnType,
	c.max_length,
	c.precision,
	c.scale
into
	#tabletypecol
from 
	sys.columns c,
	sys.types ct,
	sys.table_types tt
where 
	c.user_type_id = ct.user_type_id 
	and 
	c.object_id = tt.type_table_object_id 
order by 
	1, 3
	
-------------------------------------------------------------------------------------------------------------------------
