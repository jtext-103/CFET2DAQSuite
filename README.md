CFET2 DAQ related program

projects introduction
//------------------------------//
BaiscAIModule is an interface and comman defintion of DAQ AI, all DAQ card like JY and NI can use it.
DAQAIThing is a CFET Thing with a instance of BaiscAIModule, can control all DAQ porgrass without any dependence of a special DAQ card or file format.
DAQDataUploadThing can upload local DAQ file to another place(File copy), as same as DAQAIThing it doesn't rely any special file format.
MDSUploadThing can Upload local DAQ file to a MDS Server, it's similar to DAQDataUploadThing.
DAQsManagerView is just a web for monitor and control all DAQAIThing.
JYDAQAI and NIDAQAI is the realizations of BaiscAIModule.
ShotDirOperator is a sample class for DAQAIThing to use to manage local datafile Dir.

CFET2App repo's link:ssh://git@jtext103-PowerEdge-R410:9000/jtext103/CFET2App.git