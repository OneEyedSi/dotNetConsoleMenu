// Classes used to create objects to display.

using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectViewerDEMO
{
	public class DemoClass1
	{
		public int Class1IntField = 1111;
		public MyLittleObject Class1ObjectField = new MyLittleObject(11111, "Object field string");

		public int Class1IntProperty
		{
			get { return 1; }
		}

		public string Class1StringProperty1
		{
			get { return "First string property"; }
		}

		public string Class1StringProperty2
		{
			get { return null; }
		}

		public DemoClass2 Class1ObjectProperty1
		{
			get
			{
				return new DemoClass2(11, "First Instance String Property 1",
					new MyLittleObject(21, "MySubObject1"),
					new int[] { 11, 12, 13 }, new string[] { "First Instance String Array Element 1", 
							"First Instance String Array Element 2" },
					new List<MyLittleObject>(new MyLittleObject[] 
													{	new MyLittleObject(1, "ListObject1"), 
														new MyLittleObject(2, "ListObject2") }),
					new List<string>(new string[] { "String list item 1", "String list item 2" }),
					new List<int>(new int[] { 10, 20 })
					);
			}
		}

		public DemoClass2[] Class1ObjectArrayProperty1
		{
			get
			{
				DemoClass2[] objectArray = {
												new DemoClass2(111, "First Element String Property 1",
													new MyLittleObject(121, "MySubObject11"),
													new int[] { 111, 112, 113 }, 
													new string[] { "First Element String Array Element 1", 
														"First Element String Array Element 2" }, 
													new List<MyLittleObject>( new MyLittleObject[] 
														{	new MyLittleObject(101, "Element 1 ListObject1"), 
															new MyLittleObject(102, "Element 1 ListObject2") } ),
													new List<string>( new string[] 
														{ "Element 1 String list item 1", 
															"Element 1 String list item 2" } ),
													new List<int>( new int[] { 110, 120 })), 
												new DemoClass2(211, "Second Element String Property 1",
													new MyLittleObject(221, "MySubObject21"),
													new int[] { 211, 212, 213 }, 
													new string[] { "Second Element String Array Element 1", 
														"Second Element String Array Element 2" }, 
													new List<MyLittleObject>( new MyLittleObject[] 
														{	new MyLittleObject(201, "Element 2 ListObject1"), 
															new MyLittleObject(202, "Element 2 ListObject2") } ),
													new List<string>( new string[] 
														{ "Element 2 String list item 1", 
															"Element 2 String list item 2" } ),
													new List<int>( new int[] { 210, 220 }))
												};
				return objectArray;
			}
		}

		public DemoClass2[] Class1ObjectArrayProperty2
		{
			get { return null; }
		}

		public DemoClass2[] Class1ObjectArrayProperty3
		{
			get
			{
				return new DemoClass2[0];
			}
		}

	}

	public class DemoClass2
	{
		public DemoClass2(int intProperty, string stringProperty1,
			MyLittleObject objectProperty,
			int[] intArrayProperty, string[] stringArrayProperty,
			List<MyLittleObject> objectListProperty, List<string> stringListProperty,
			List<int> intListProperty)
		{
			_int = intProperty;
			_string1 = stringProperty1;
			_string2 = null;
			_parameter = objectProperty;
			_parameter2 = null;
			_intArray = intArrayProperty;
			_intArray2 = null;
			_intArray3 = new int[0];
			_stringArray = stringArrayProperty;
			_stringArray2 = null;
			_stringArray3 = new string[0];
			_objectList1 = objectListProperty;
			_objectList2 = null;
			_objectList3 = new List<MyLittleObject>();
			_stringList1 = stringListProperty;
			_stringList2 = null;
			_stringList3 = new List<string>();
			_intList1 = intListProperty;
			_intList2 = null;
			_intList3 = new List<int>();
		}

		private int _int;
		public int IntProperty
		{
			get { return _int; }
		}

		private string _string1;
		public string StringProperty1
		{
			get { return _string1; }
		}

		private string _string2;
		public string StringProperty2
		{
			get { return _string2; }
		}

		private MyLittleObject _parameter;
		public MyLittleObject ObjectProperty
		{
			get { return _parameter; }
		}

		private MyLittleObject _parameter2;
		public MyLittleObject ObjectProperty2
		{
			get { return _parameter2; }
		}

		private int[] _intArray;
		public int[] IntArrayProperty
		{
			get { return _intArray; }
		}

		private int[] _intArray2;
		public int[] IntArrayProperty2
		{
			get { return _intArray2; }
		}

		private int[] _intArray3;
		public int[] IntArrayProperty3
		{
			get { return _intArray3; }
		}

		private string[] _stringArray;
		public string[] StringArrayProperty
		{
			get { return _stringArray; }
		}

		private string[] _stringArray2;
		public string[] StringArrayProperty2
		{
			get { return _stringArray2; }
		}

		private string[] _stringArray3;
		public string[] StringArrayProperty3
		{
			get { return _stringArray3; }
		}

		private List<MyLittleObject> _objectList1;
		public List<MyLittleObject> ObjectList1
		{
			get { return _objectList1; }
		}

		private List<MyLittleObject> _objectList2;
		public List<MyLittleObject> ObjectList2
		{
			get { return _objectList2; }
		}

		private List<MyLittleObject> _objectList3;
		public List<MyLittleObject> ObjectList3
		{
			get { return _objectList3; }
		}

		private List<string> _stringList1;
		public List<string> StringList1
		{
			get { return _stringList1; }
		}

		private List<string> _stringList2;
		public List<string> StringList2
		{
			get { return _stringList2; }
		}

		private List<string> _stringList3;
		public List<string> StringList3
		{
			get { return _stringList3; }
		}

		private List<int> _intList1;
		public List<int> IntList1
		{
			get { return _intList1; }
		}

		private List<int> _intList2;
		public List<int> IntList2
		{
			get { return _intList2; }
		}

		private List<int> _intList3;
		public List<int> IntList3
		{
			get { return _intList3; }
		}
	}

	public class MyLittleObject
	{
		private int _myInt;
		public int MyProperty
		{
			get { return _myInt; }
		}

		private string _myString;
		public string MyStringProperty
		{
			get { return _myString; }
		}

		public MyLittleObject(int myInt, string myString)
		{
			_myInt = myInt;
			_myString = myString;
		}
	}

	public class RecursiveObject
	{
		public RecursiveObject(int integerProperty)
		{
			_integerProperty = integerProperty;
		}

		private int _integerProperty;
		public int IntegerProperty
		{
			get { return _integerProperty; }
		}

		public RecursiveObject RecursiveProperty
		{
			get { return new RecursiveObject(_integerProperty + 1); }
		}

		public RecursiveObject2 ObjectProperty
		{
			get { return new RecursiveObject2(_integerProperty); }
		}
	}

	public class RecursiveObject2
	{
		public RecursiveObject2(int integerProperty2)
		{
			_integerProperty2 = integerProperty2;
		}

		private int _integerProperty2;
		public int IntegerProperty2
		{
			get { return _integerProperty2; }
		}

		public RecursiveObject RecursiveProperty2
		{
			get { return new RecursiveObject(_integerProperty2 + 1); }
		}
	}
}
