using System;
using System.Linq;
using HtmlForgeX;
using DomainDetective;
using DomainDetective.Reports.Models;
using DomainDetective.Reports.Enums;

namespace DomainDetective.Reports.Html;

/// <summary>
/// Simplified domain security report generator using HtmlForgeX
/// </summary>
public class SimpleDomainReport {
    private readonly DomainHealthCheck _healthCheck;
    private readonly string _domain;

    public SimpleDomainReport(DomainHealthCheck healthCheck, string domain) {
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

            // Score Summary Card
            page.Row(row => {
                row.Column(TablerColumnNumber.Twelve, col => {
                    col.Card(card => {
                        card.Header(header => {
                            header.Title("Security Score Overview");
                        });
                        card.Body(body => {
                            body.Row(scoreRow => {
                                // Overall Score
                                scoreRow.Column(TablerColumnNumber.Four, scoreCol => {
                                    var score = CalculateOverallScore();
                                    scoreCol.Card(innerCard => {
                                        innerCard.Background(GetScoreColor(score), "#FFFFFF")
                                                .Header(h => h.Title("Overall Score"))
                                                .Body(b => {
                                                    b.H1($"{score}/100");
                                                    b.Text(GetRiskLevel(score)).Weight(TablerFontWeight.Medium);
                                                });
                                    });
                                });

                                // Key Metrics
                                scoreRow.Column(TablerColumnNumber.Eight, metricsCol => {
                                    metricsCol.DataGrid(grid => {
                                        grid.AsCompact();

                                        // Email Security
                                        var spfStatus = _healthCheck.SpfAnalysis?.SpfRecordExists == true && _healthCheck.SpfAnalysis?.StartsCorrectly == true ? "✅ Pass" : "❌ Fail";
                                        grid.AddItem("SPF", spfStatus);

                                        var dmarcStatus = _healthCheck.DmarcAnalysis?.DmarcRecordExists == true ? "✅ Pass" : "❌ Fail";
                                        grid.AddItem("DMARC", dmarcStatus);

                                        var dkimStatus = _healthCheck.DKIMAnalysis?.AnalysisResults?.Count > 0 ? "✅ Pass" : "❌ Fail";
                                        grid.AddItem("DKIM", dkimStatus);

                                        // Infrastructure
                                        var dnssecStatus = _healthCheck.DnsSecAnalysis?.DsRecords?.Count > 0 ? "✅ Enabled" : "⚠️ Disabled";
                                        grid.AddItem("DNSSEC", dnssecStatus);

                                        var mxStatus = _healthCheck.MXAnalysis?.MxRecordExists == true ? "✅ Valid" : "⚠️ Issues";
                                        grid.AddItem("MX Records", mxStatus);

                                        var tlsStatus = "⚠️ Unknown";
                                        if (_healthCheck.SmtpTlsAnalysis?.ServerResults?.Count > 0) {
                                            var anyCertValid = _healthCheck.SmtpTlsAnalysis.ServerResults.Values.Any(r => r.CertificateValid);
                                            tlsStatus = anyCertValid ? "✅ Secure" : "⚠️ Issues";
                                        }
                                        grid.AddItem("TLS", tlsStatus);
                                    });
                                });
                            });
                        });
                    });
                });
            });

            // Detailed Results
            page.Divider("Detailed Analysis");

            // Email Authentication
            page.Row(row => {
                row.Column(TablerColumnNumber.Six, col => {
                    col.Card(card => {
                        card.Header(h => h.Title("Email Authentication"));
                        card.Body(body => {
                            body.DataGrid(grid => {
                                // SPF
                                if (_healthCheck.SpfAnalysis != null) {
                                    grid.AddItem("SPF Record", _healthCheck.SpfAnalysis.SpfRecordExists && _healthCheck.SpfAnalysis.StartsCorrectly ? "Valid" : "Invalid");
                                    grid.AddItem("SPF Status", _healthCheck.SpfAnalysis.SpfRecordExists ? "Found" : "Not found");
                                }

                                // DMARC
                                if (_healthCheck.DmarcAnalysis != null) {
                                    grid.AddItem("DMARC Policy", _healthCheck.DmarcAnalysis.Policy ?? "None");
                                    grid.AddItem("DMARC Status", _healthCheck.DmarcAnalysis.DmarcRecordExists ? "Found" : "Not found");
                                }

                                // DKIM
                                if (_healthCheck.DKIMAnalysis != null) {
                                    var dkimValid = _healthCheck.DKIMAnalysis.AnalysisResults?.Any(kvp => kvp.Value.DkimRecordExists && kvp.Value.StartsCorrectly && kvp.Value.PublicKeyExists) ?? false;
                                    grid.AddItem("DKIM Valid", dkimValid ? "Yes" : "No");
                                    var selectorCount = _healthCheck.DKIMAnalysis.AnalysisResults?.Count ?? 0;
                                    grid.AddItem("DKIM Selectors", selectorCount.ToString());
                                }
                            });
                        });
                    });
                });

                row.Column(TablerColumnNumber.Six, col => {
                    col.Card(card => {
                        card.Header(h => h.Title("DNS Security"));
                        card.Body(body => {
                            body.DataGrid(grid => {
                                // DNSSEC
                                if (_healthCheck.DnsSecAnalysis != null) {
                                    var hasDnssec = _healthCheck.DnsSecAnalysis.DsRecords?.Count > 0;
                                    grid.AddItem("DNSSEC", hasDnssec ? "Enabled" : "Disabled");
                                    grid.AddItem("DS Records", _healthCheck.DnsSecAnalysis.DsRecords?.Count.ToString() ?? "0");
                                }

                                // NS
                                if (_healthCheck.NSAnalysis != null) {
                                    grid.AddItem("NS Records", _healthCheck.NSAnalysis.NsRecordExists ? "Found" : "Not found");
                                    grid.AddItem("NS Status", _healthCheck.NSAnalysis.NsRecordExists ? "Valid" : "Invalid");
                                }

                                // MX
                                if (_healthCheck.MXAnalysis != null) {
                                    grid.AddItem("MX Records", _healthCheck.MXAnalysis.MxRecordExists ? "Found" : "Not found");
                                    grid.AddItem("MX Status", _healthCheck.MXAnalysis.MxRecordExists ? "Valid" : "Invalid");
                                }
                            });
                        });
                    });
                });
            });

            // Recommendations
            page.Divider("Recommendations");

            page.Row(row => {
                row.Column(TablerColumnNumber.Twelve, col => {
                    col.Card(card => {
                        card.Header(h => h.Title("Priority Actions"));
                        card.Body(body => {
                            var recommendations = GenerateRecommendations();

                            if (recommendations.Any()) {
                                body.AddList(list => {
                                    list.WithItems(items => {
                                        foreach (var rec in recommendations.Take(5)) {
                                            items.Item(rec);
                                        }
                                    });
                                });
                            } else {
                                body.Text("✅ No critical issues found. Good job!");
                            }
                        });
                    });
                });
            });
        });

        document.Save(outputPath, openInBrowser);
    }

    private int CalculateOverallScore() {
        var score = 100;

        // Deduct points for issues
        if (!(_healthCheck.SpfAnalysis?.SpfRecordExists == true && _healthCheck.SpfAnalysis?.StartsCorrectly == true)) score -= 10;
        if (_healthCheck.DmarcAnalysis?.DmarcRecordExists != true) score -= 15;
        var hasValidDkim = _healthCheck.DKIMAnalysis?.AnalysisResults?.Any(kvp => kvp.Value.DkimRecordExists && kvp.Value.StartsCorrectly && kvp.Value.PublicKeyExists) ?? false;
        if (!hasValidDkim) score -= 10;
        if (!(_healthCheck.DnsSecAnalysis?.DsRecords?.Count > 0)) score -= 10;
        if (_healthCheck.MXAnalysis?.MxRecordExists != true) score -= 5;
        if (_healthCheck.SmtpTlsAnalysis?.ServerResults?.Count > 0) {
            var anyCertValid = _healthCheck.SmtpTlsAnalysis.ServerResults.Values.Any(r => r.CertificateValid);
            if (!anyCertValid) score -= 10;
        } else {
            score -= 10; // No TLS data
        }

        return Math.Max(0, score);
    }

    private string GetScoreColor(int score) {
        return score >= 80 ? "#10B981" :
               score >= 60 ? "#F59E0B" :
               score >= 40 ? "#F97316" : "#EF4444";
    }

    private string GetRiskLevel(int score) {
        return score >= 80 ? "Low Risk" :
               score >= 60 ? "Medium Risk" :
               score >= 40 ? "High Risk" : "Critical Risk";
    }

    private string[] GenerateRecommendations() {
        var recommendations = new System.Collections.Generic.List<string>();

        if (_healthCheck.DmarcAnalysis?.Policy == null || _healthCheck.DmarcAnalysis.Policy == "none") {
            recommendations.Add("🔴 Implement DMARC policy to protect against email spoofing");
        }

        if (!(_healthCheck.SpfAnalysis?.SpfRecordExists == true && _healthCheck.SpfAnalysis?.StartsCorrectly == true)) {
            recommendations.Add("🟡 Fix SPF record issues to improve email deliverability");
        }

        var hasValidDkim = _healthCheck.DKIMAnalysis?.AnalysisResults?.Any(kvp => kvp.Value.DkimRecordExists && kvp.Value.StartsCorrectly && kvp.Value.PublicKeyExists) ?? false;
        if (!hasValidDkim) {
            recommendations.Add("🟡 Configure DKIM signing for email authentication");
        }

        if (!(_healthCheck.DnsSecAnalysis?.DsRecords?.Count > 0)) {
            recommendations.Add("🟠 Enable DNSSEC to protect against DNS spoofing");
        }

        if (_healthCheck.SmtpTlsAnalysis?.ServerResults?.Count > 0) {
            var anyCertValid = _healthCheck.SmtpTlsAnalysis.ServerResults.Values.Any(r => r.CertificateValid);
            if (!anyCertValid) {
                recommendations.Add("🟡 Fix TLS configuration issues for secure email transport");
            }
        } else {
            recommendations.Add("🟡 Configure TLS for secure email transport");
        }

        return recommendations.ToArray();
    }
}