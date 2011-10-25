<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	exclude-result-prefixes="xsl">

	<xsl:output method="text" version="1.0" encoding="UTF-8" indent="no" />
	<xsl:strip-space elements="*" />
	
	<xsl:template match="/Tokens">
		<xsl:text>
/*
	This file is auto-generated. Do NOT modify. Modify Tokens.xml instead.
*/

using System;

namespace Cirrus.Tools.Cilc.Targets.Web {

	public static class Tokens {
		</xsl:text>
	<xsl:apply-templates select="T" />
	<xsl:text>
	}
}</xsl:text>
	</xsl:template>
	
	<xsl:template match="T">
		<xsl:text>public static readonly Token </xsl:text>
		<xsl:value-of select="@Name" />
		<xsl:text> = new Token ("</xsl:text>
		<xsl:value-of select="@Js" disable-output-escaping="yes" />
		<xsl:text>", </xsl:text>
		<xsl:value-of select="@Cwp" />
		<xsl:text>);
		</xsl:text>
	</xsl:template>
	
</xsl:stylesheet>
