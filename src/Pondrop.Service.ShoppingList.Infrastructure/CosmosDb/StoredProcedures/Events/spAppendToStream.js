function appendToStream(streamId, expectedVersion, events) {
    const parsedEvents = JSON.parse(events);
    if (!parsedEvents || parsedEvents.length < 1) {
        throw new Error("Unable to parse events.");
    }
    
    if (!parsedEvents.every(event => event.streamId === streamId)) {
        throw new Error("Added events cannot cross streams.");
    }
    
    var versionQuery =
        {
            'query' : 'SELECT Max(c.sequenceNumber) FROM c WHERE c.streamId = @streamId',
            'parameters' : [{ 'name': '@streamId', 'value': streamId }]
        };

    const success = __.queryDocuments(__.getSelfLink(), versionQuery,
        function (err, items, options) {
            if (err) {
                throw new Error("Unable to get stream sequence: " + err.message);
            }

            if (!items || !items.length) {
                throw new Error("No results from stream query.");
            }

            const latestSeqNum = items[0].$1;

            // Concurrency check.
            if ((!latestSeqNum && expectedVersion === 0) || (latestSeqNum === expectedVersion))
            {
                // Everything's fine, bulk insert the events.
                parsedEvents.forEach(event => __.createDocument(__.getSelfLink(), event));
                __.response.setBody(parsedEvents.length);
            }
            else {
                __.response.setBody(0);
            }
        });

    if (!success) {
        throw new Error('Appending events failed.');
    }        
}
