# MapNoReduce Platform #

Check this in **[BitBucket Cards](http://www.bitbucketcards.com)**.

The goal of this project is to design and implement MapNoReduce, a simplified implementation of the MapReduce middleware and programming model. MapReduce was introduced by Google in 2004 and is currently one of the most popular approaches for large scale data analytics also thanks to the availability of high quality open-source implementations (e.g., Hadoop).

When using the MapReduce paradigm, the computation takes a set of input key/value pairs, and produces a set of output key/value pairs. MapReduce users express the computation as two functions: Map and Reduce. For simplicity, in this project students will only implement the Map part of MapReduce. The Map function (different for each application), is written by the user and takes an input set of key/value pairs and produces a set of key/value pairs. In the case of MapNoReduce, the input key/value pairs are extracted from input files. The keys are the numbers of the line of the file being read and the values are the content of those lines.

The Map invocations are distributed across multiple machines by automatically parti- tioning the input data into a set of splits of size S. The input splits can be processed in parallel by different machines, named workers. The system should ensure that for each job submitted, all the input data is processed. Furthermore, the system should strive to ensure good performance my monitoring a job’s progress, detecting faulty or slow machines and rescheduling their tasks on idle machines.

In the original MapReduce implementation there is a centralised component, called the job tracker, that is in charge of supervising the progress of the job. In MapNoReduce, the job tracker functionalities are implemented, in a distributed manner, by the workers, that cooperate to provide the tracker’s functionalities. It is up to the students to decide how to distribute the tracker’s tasks among the workers. The purpose of this exercise is to allow the students to get a better grasp on the advantages and limitations of distribution.
#
#
![Screen Shot 2015-03-27 at 01.08.58.png](https://bitbucket.org/repo/kAEL4r/images/718225642-Screen%20Shot%202015-03-27%20at%2001.08.58.png)
#
#
### How do I get set up? ###

* Install Visual Studio 2013
* Launch Puppet Master UI, which launches a Puppet Master Server Daemon in background
* You are ready!

### Contribution guidelines ###

* Writing tests
* Code review
* Document code

### Who do I talk to? ###

* João Pinho (jpepinho [at] gmail [dot] com)
* Cláudia Filipe (claudiabfilipe [at] gmail [dot] com)
* Diogo Rosa (diogomcrosa [at] hotmail [dot] com)