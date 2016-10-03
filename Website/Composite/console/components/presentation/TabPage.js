// Shows page if given name and type, else shows nothing.
import React, { PropTypes } from 'react';

const TabPage = props => {
	if (!props.pageDef || !props.pageDef.type) {
		return <div/>;
	} else {
		let Page = props.pageTypes[props.pageDef.type];
		if (!Page) {
			throw new Error('Could not find page type "' + props.pageDef.type + '" for page "' + props.pageDef.name + '"');
		}
		let otherProps = Object.assign({}, props);
		delete otherProps.pageTypes;
		return (<Page {...otherProps}/>);
	}
};

TabPage.propTypes = {
	pageDef: PropTypes.object.isRequired,
	pageTypes: PropTypes.objectOf(PropTypes.func).isRequired
};

export default TabPage;