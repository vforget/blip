Project Description
===================

BL!P [blip], or BLAST in Pivot, is a computer program that automates the NCBI BLAST alignment of coding DNA or protein sequences and processes the results for visualization in the Microsoft Live Labs program Pivot.

Download BL!P
=============

Downlad Setup.msi from the bin/ directory.

Software requirements
=====================

 * Microsoft Windows XP or better
 * [Microsoft .NET 4.0](http://www.microsoft.com/en-us/download/details.aspx?id=17851)
 * [Microsoft Silverlight](http://www.microsoft.com/getsilverlight/Get-Started/Install/Default.aspx)
 * [Microsoft Silverlight PivotViewer Control](http://www.microsoft.com/en-us/download/details.aspx?id=17747)

 

[Explore a sample data set](http://www.genomequebec.mcgill.ca/compgen/public/blip/blip_demo/blip.html)

[Watch a short video presentation](http://research.microsoft.com/apps/video/default.aspx?id=142016)

[Question to ask, or bug to report?](https://github.com/vforget)

Introduction
============

[NCBI BLAST](http://blast.ncbi.nlm.nih.gov/Blast.cgi) is a popular software program used to find regions of similarity between biological sequences, 
and can be used to infer functional and evolutionary relationships between sequences. A NCBI BLAST  search using multiple query sequences 
(e.g. gene predictions from a genome sequencing project) typically generates a large dataset that must be explored for functional or 
evolutionary patterns on interest. Current approaches to exploring NCBI BLAST results  include automated filtering of the dataset using a priori 
significance thresholds followed by manual inspection. While this approach is satisfactory, novel data exploration and visualization software 
exists that allows for patterns to be identified more easily  and with less bias. One such program is 
[Pivot](http://www.silverlight.net/learn/pivotviewer/), which can visualize the relationship between pieces of information allowing for the discovery 
of hidden patterns. Pivot structures its data into collections, which combines groups of similar items based on values of certain attributes
(facet categories), and represents each item using an image. We have created a software application, BL!P, that automates the NCBI BLAST search 
of multiple biological sequences and converts the results into a Pivot collection. BL!P also provides an interface  to construct custom image 
layouts for the collection of Pivot items.

BL!P was developed using C# and .NET 4.0, and uses the [Microsoft Biology Foundation](http://mbf.codeplex.com) (MBF) bioinformatics toolkit to 
access NCBI resources such as NCBI BLAST and [GenBank](http://www.ncbi.nlm.nih.gov/genbank/), as well as parsers to read/write biological sequence data.

BL!P automatically submits multiple FASTA formatted coding DNA or 
amino acid sequences to a NCBI BLAST protein database. Submissions are polled until complete, and the results are saved to disk for later use. 
Upon completion of the NCBI BLAST search, the GenBank  records for each BLAST hit that meets user specified criteria is downloaded and saved to 
disk for later use. The results from BLAST and information in the GenBank records are parsed and converted to a Pivot collection. Using data from 
the Pivot collection,  a custom image layout is constructed to represent each BLAST hit. The results are saved to disk and can be loaded into 
Pivot for exploration. BL!P is a member of the [Microsoft Biology Initiative](http://research.microsoft.com/bio/).

Developers
==========

Vince Forgetta
Postdoctoral Researcher, McGill University.

Leonardo Montes Marin
Ingeniero Investigador en Centro de Bioinformática y Biología Computacional de Colombia
