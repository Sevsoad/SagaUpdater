# SagaUpdater

## RavenDbWorker
Used to change namespaces in IDs of SagaUniqueIdentity collection

## SagaMigrator
Used to migrate NServiceBus sagas, including cases when data entity namespace was updated.
Tested on 1,2 versions of Raven
####How it works:
1. To move saga, you need to copy all saga records from '%YourSagaName%' collection in database, also you need to copy all associated with this saga records in SagaUniqueIdentity collection.
	Moved sagas will not work, if there was a change of namespace while moving saga data entity class to new namespace in code.
2. SagaMigrator is used to migrate sagas with all required namespace changes, like change of namespace in UniqueIdentity and in RavenClrType.
