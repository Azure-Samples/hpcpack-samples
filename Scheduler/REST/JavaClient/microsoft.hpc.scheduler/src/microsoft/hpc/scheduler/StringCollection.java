package microsoft.hpc.scheduler;

import java.io.InputStream;
import java.util.List;
import java.util.Vector;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Unmarshaller;
import javax.xml.transform.stream.StreamSource;

import com.microsoft.schemas.hpcs2008r2.common.ArrayOfObject;

/**
 * StringCollection implementation for HPC REST
 * @author japrom
 *
 */
public class StringCollection extends Vector<String> implements IStringCollection {

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	/**
	 * Converts ArrayOfObject into StringCollection by ignoring the name and saving only the values
	 * Useful for the Get Clusters REST API, mainly
	 * @param xmlStream
	 */
	StringCollection(InputStream xmlStream)
	{
		super();
		
		try {
			StreamSource source = new StreamSource(xmlStream);
			
			//
			// Deserialize to object of ArrayOfObject
			//
			JAXBContext jc = JAXBContext.newInstance(ArrayOfObject.class);
			Unmarshaller u = jc.createUnmarshaller();
			ArrayOfObject arrayOfObject = (ArrayOfObject) u.unmarshal(source);
			
			//
			// Take all the values and add to string collection (ignoring keys)
			//
			List<ArrayOfObject.Object> objectList = arrayOfObject.getObject();
			for(ArrayOfObject.Object item : objectList)
			{
				List<ArrayOfObject.Object.Properties.Property> properties = item.getProperties().getProperty();
				for(ArrayOfObject.Object.Properties.Property property : properties)
				{
					this.add(property.getValue());
				}
			}
			
		} catch (JAXBException e ) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	/**
	 * Converts a comma separated list into a string collection
	 * @param commaSeparatedList
	 */
	StringCollection(String commaSeparatedList)
	{
		super();
		
		String[] _stringList = commaSeparatedList.split(",");
		for(String item : _stringList)
		{
			this.add(item);
		}
	}
	
	public StringCollection() 
	{
		super();
	}
}
