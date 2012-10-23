/* 
 * Copyright 2008-2011 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: Justin Fyfe
 * Date: 10-22-2012
 */
package ca.marc.everest.formatters.xml.its1;

import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Comparator;
import java.util.List;

import javax.swing.event.RowSorterEvent.Type;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamWriter;

import ca.marc.everest.annotations.Properties;
import ca.marc.everest.annotations.Property;
import ca.marc.everest.annotations.PropertyType;
import ca.marc.everest.annotations.Structure;
import ca.marc.everest.annotations.StructureType;
import ca.marc.everest.interfaces.IGraphable;
import ca.marc.everest.interfaces.IResultDetail;
import ca.marc.everest.interfaces.ResultCodeType;

/**
 * A formatter that uses reflection to format instances
 */
public class ReflectionFormatter {

	// Backing field for host
	private XmlIts1Formatter m_host;
	
	/**
	 * Gets the host of the formatter
	 */
	public XmlIts1Formatter getHost() { return this.m_host; }
	/**
	 * Sets the host of this formatter
	 */
	public void setHost(XmlIts1Formatter value) { this.m_host = value; }
	
	/**
	 * Validate the instance x returning validation errors
	 */
	public Collection<IResultDetail> validate(IGraphable target, String locationPath)
	{
		return null;
	}
	
	/**
	 * Graph o onto xw returning the results
	 */
	public ResultCodeType graph(XMLStreamWriter xw, Object o, IGraphable context, XmlIts1FormatterGraphResult resultContext)
	{
	
		ResultCodeType retVal = ResultCodeType.Accepted;
		Class<?> instanceType = o.getClass();
		
		// Verify that the passed instance is not null
		if(o == null)
			throw new IllegalArgumentException();
		
		// Attempt to get the nullflavor
		Method nfp = null;
		try {
			nfp = o.getClass().getMethod("getNullFlavor");
		} catch (NoSuchMethodException e) {
			// TODO: add to result detail
		} catch (SecurityException e) {
			// TODO: add to result detail
		}

		
		// Execution
		try {

			// Determine null flavor
			boolean isInstanceNull = false, 
					isEntryPoint = false;
			if(nfp != null)
				isInstanceNull = nfp.invoke(o) != null;
			
			// Get structureAttribute
			Structure struct = (Structure)instanceType.getAnnotation(Structure.class);
			if(struct != null && struct.structureType().equals(StructureType.INTERACTION))
			{
				isEntryPoint = true;
				xw.writeStartElement("hl7", struct.name(), XmlIts1Formatter.NS_HL7);
				xw.writeAttribute("ITSVersion", "XML_1.0");
				xw.writeNamespace("hl7", XmlIts1Formatter.NS_HL7);
				xw.writeNamespace("xsi", XmlIts1Formatter.NS_XSI);
			}
			// TODO: Entry points, how do I determine if there is already data on the writer?
			
			// Reflect the properties and ensure they are in the appropriate order
            List<Method> buildProperties = getBuildProperties(instanceType);
            
		} catch (IllegalAccessException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IllegalArgumentException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (InvocationTargetException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (XMLStreamException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		return retVal;
	}
	
	/**
	 * Determine if the method is a traversable association
	 */
	private boolean isOfPropertyType(Method m, PropertyType type)
	{
		Property propertyAnnotation = m.getAnnotation(Property.class);
		Properties propertiesAnnotation = m.getAnnotation(Properties.class);
		
		if(propertyAnnotation != null)
			return propertyAnnotation.propertyType().equals(type);
		else if(propertiesAnnotation != null)
			return propertiesAnnotation.value()[0].propertyType().equals(type);
		return false;
	}
	
	/**
	 * Build a list of methods
	 */
	private List<Method> getBuildProperties(Class<?> instanceType) {
		// Arrays of items representing the property types
		// Structural, NonStructural and Traversable Associations
		// These will be used to ensure that the return array is ordered correctly
		// such that XML instances will 
		List<Method> structural = new ArrayList<Method>(10),
				nonStructural = new ArrayList<Method>(10),
				traversable = new ArrayList<Method>(10);

		// Find all methods with property annotation
		Method[] thisMethods = instanceType.getMethods();
		
		// Sort the array of methods by the sort key
		Arrays.sort(thisMethods, new Comparator<Method>() {

			@Override
			public int compare(Method a, Method b) {
				Property propertyAnnotation = a.getAnnotation(Property.class);
				Properties propertiesAnnotation = a.getAnnotation(Properties.class);
				
				// Sort key for a
				Integer aSortKey = 0; 
				if(propertyAnnotation != null)
					aSortKey = propertyAnnotation.sortKey();
				else if(propertiesAnnotation != null)
					aSortKey = propertiesAnnotation.value()[0].sortKey();
				
				propertyAnnotation = a.getAnnotation(Property.class);
				propertiesAnnotation = a.getAnnotation(Properties.class);
				
				// Sort key for b
				Integer bSortKey = 0; 
				if(propertyAnnotation != null)
					bSortKey = propertyAnnotation.sortKey();
				else if(propertiesAnnotation != null)
					bSortKey = propertiesAnnotation.value()[0].sortKey();

				return aSortKey.compareTo(bSortKey);
			}
			
		});
		
		// Iterate through methods and prepare the output arrays
		for(Method meth : thisMethods)
		{
			if(this.isOfPropertyType(meth, PropertyType.TRAVERSABLEASSOCIATION))
				traversable.add(0, meth);
			else if(this.isOfPropertyType(meth, PropertyType.NONSTRUCTURAL))
				nonStructural.add(0, meth);
			else
				structural.add(0, meth);
		}
		
		// Return value
		List<Method> retVal = new ArrayList<Method>();
		retVal.addAll(structural);
		retVal.addAll(nonStructural);
		retVal.addAll(traversable);
		
		return retVal;
	}
}