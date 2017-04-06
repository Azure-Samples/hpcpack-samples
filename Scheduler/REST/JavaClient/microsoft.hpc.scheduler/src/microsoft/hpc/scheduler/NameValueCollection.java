package microsoft.hpc.scheduler;

import java.util.Hashtable;
import java.util.List;


import com.microsoft.schemas.hpcs2008r2.common.ArrayOfProperty;
import com.microsoft.schemas.hpcs2008r2.common.ArrayOfProperty.Property;

public class NameValueCollection extends Hashtable<String, String> implements INameValueCollection {

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	/**
	 * Initialize a NameValueCollection from ArrayOfProperty. Makes searching a lot easier
	 * @param arrayOfProperty
	 */
	NameValueCollection(ArrayOfProperty arrayOfProperty)
	{
		super();
		//
		// Take all the names and values and add it to the hashtable
		//
		List<Property> objectList = arrayOfProperty.getProperty();
		
		for(Property item : objectList)
		{
			this.put(item.getName(), item.getValue());
		}
	}
	
	/**
	 * Converts a NameValueCollection back to ArrayOfProperty
	 * @param collection
	 * @return
	 */
	static ArrayOfProperty toArrayOfProperty(INameValueCollection collection)
	{
		ArrayOfProperty arr = new ArrayOfProperty();
		for(String name : collection.keySet())
		{
			Property property = new Property();
			property.setName(name);
			property.setValue(collection.get(name));
			
			arr.getProperty().add(property);
		}
		return arr;
	}
	
	public NameValueCollection() {
		super();
	}
}
