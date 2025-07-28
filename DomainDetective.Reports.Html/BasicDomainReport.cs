using System;
using System.Linq;
using HtmlForgeX;
using DomainDetective;
using DomainDetective.Reports.Models;
using DomainDetective.Reports.Enums;

namespace DomainDetective.Reports.Html;

/// <summary>
/// Basic domain security report generator that compiles without errors
/// </summary>
public class BasicDomainReport {
    private readonly DomainHealthCheck _healthCheck;
    private readonly string _domain;

    public BasicDomainReport(DomainHealthCheck healthCheck, string domain) {
        _healthCheck = healthCheck;
        _domain = domain;
    }

    public void GenerateReport(string outputPath, bool openInBrowser = true) {
        using var document = new Document {
            Head = {
                Title = $"Security Report - {_domain}",
                Author = "DomainDetective",
                Revised = DateTime.Now
            },
            LibraryMode = LibraryMode.Online,
            ThemeMode = ThemeMode.Light
        };

        document.Body.Page(page => {
            page.Layout = TablerLayout.Fluid;
            
            // Header
            page.H1($"🛡️ Security Report for {_domain}");
            page.Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}").Style(TablerTextStyle.Muted);
            
            // Main content card
            page.Row(row => {
                row.Column(TablerColumnNumber.Twelve, col => {
                    col.Card(card => {
                        card.Header(header => {
                            header.Title("Domain Security Analysis");
                        });
                        card.Body(body => {
                            body.DataGrid(grid => {
                                grid.AsCompact();
                                
                                // SPF
                                if (_healthCheck.SpfAnalysis != null) {
                                    var spfStatus = _healthCheck.SpfAnalysis.SpfRecordExists ? "✅ Found" : "❌ Not found";
                                    grid.AddItem("SPF Record", spfStatus);
                                    if (_healthCheck.SpfAnalysis.SpfRecordExists) {
                                        grid.AddItem("SPF Valid", _healthCheck.SpfAnalysis.StartsCorrectly ? "Yes" : "No");
                                    }
                                }
                                
                                // DMARC
                                if (_healthCheck.DmarcAnalysis != null) {
                                    var dmarcStatus = _healthCheck.DmarcAnalysis.DmarcRecordExists ? "✅ Found" : "❌ Not found";
                                    grid.AddItem("DMARC Record", dmarcStatus);
                                    if (_healthCheck.DmarcAnalysis.DmarcRecordExists) {
                                        grid.AddItem("DMARC Policy", _healthCheck.DmarcAnalysis.Policy ?? "None");
                                    }
                                }
                                
                                // DKIM
                                if (_healthCheck.DKIMAnalysis != null && _healthCheck.DKIMAnalysis.AnalysisResults != null) {
                                    var dkimCount = _healthCheck.DKIMAnalysis.AnalysisResults.Count;
                                    grid.AddItem("DKIM Selectors", dkimCount.ToString());
                                    
                                    var validDkim = _healthCheck.DKIMAnalysis.AnalysisResults.Values
                                        .Any(a => a.DkimRecordExists && a.StartsCorrectly && a.PublicKeyExists);
                                    grid.AddItem("DKIM Valid", validDkim ? "✅ Yes" : "❌ No");
                                }
                                
                                // MX
                                if (_healthCheck.MXAnalysis != null) {
                                    var mxStatus = _healthCheck.MXAnalysis.MxRecordExists ? "✅ Found" : "❌ Not found";
                                    grid.AddItem("MX Records", mxStatus);
                                }
                                
                                // NS
                                if (_healthCheck.NSAnalysis != null) {
                                    var nsStatus = _healthCheck.NSAnalysis.NsRecordExists ? "✅ Found" : "❌ Not found";
                                    grid.AddItem("NS Records", nsStatus);
                                }
                                
                                // DNSSEC - Check for DS records
                                if (_healthCheck.DnsSecAnalysis != null) {
                                    var hasDsRecords = _healthCheck.DnsSecAnalysis.DsRecords?.Count > 0;
                                    grid.AddItem("DNSSEC", hasDsRecords ? "✅ DS Records found" : "⚠️ No DS Records");
                                }
                            });
                        });
                    });
                });
            });
            
            // Summary
            page.Divider("Summary");
            
            page.Row(row => {
                row.Column(TablerColumnNumber.Twelve, col => {
                    col.Card(card => {
                        card.Header(h => h.Title("Security Summary"));
                        card.Body(body => {
                            body.Text("This report provides a basic overview of your domain's security configuration.");
                            body.Text("For detailed recommendations, please review each security protocol.");
                        });
                    });
                });
            });
        });

        document.Save(outputPath, openInBrowser);
    }
}