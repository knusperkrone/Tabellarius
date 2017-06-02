namespace Tabellarius.Database
{
    public abstract class DatabaseTable
	{
		protected readonly static DatabaseAdapter db = DatabaseAdapter.GetInstance();

		public readonly string TableName;

		public abstract int Veranstaltungs_ID { get; }

		public DatabaseTable() { }

		public DatabaseTable(string tableName)
		{
			this.TableName = tableName;
		}

		public abstract string SETString();

		public abstract string IdString();

		public abstract void Update(DatabaseTable newTable);
	}
}