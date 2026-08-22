import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';
import { Landmark, Calculator, Plus, CheckCircle2, XCircle, ShieldAlert, DollarSign } from 'lucide-react';

export const LoansPage = () => {
  const { user } = useAuth();
  const [loans, setLoans] = useState([]);
  const [accounts, setAccounts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // EMI Calculator State
  const [calcPrincipal, setCalcPrincipal] = useState(25000);
  const [calcRate, setCalcRate] = useState(8.5);
  const [calcTenure, setCalcTenure] = useState(36);
  const [calcResult, setCalcResult] = useState(null);

  // Loan Application Modal
  const [showApplyModal, setShowApplyModal] = useState(false);
  const [loanType, setLoanType] = useState('Personal');
  const [appPrincipal, setAppPrincipal] = useState(10000);
  const [appTenure, setAppTenure] = useState(24);

  // Repayment Modal
  const [repayLoan, setRepayLoan] = useState(null);
  const [repayAccount, setRepayAccount] = useState('');
  const [repayAmount, setRepayAmount] = useState(0);

  const fetchLoans = async () => {
    setLoading(true);
    try {
      if (user?.customerId) {
        const res = await api.get(`/loan/customer/${user.customerId}`);
        setLoans(res.data);

        const accRes = await api.get(`/account/customer/${user.customerId}`);
        setAccounts(accRes.data);
        if (accRes.data.length > 0) setRepayAccount(accRes.data[0].accountNumber);
      } else if (user?.role === 'Admin') {
        const res = await api.get('/loan/all');
        setLoans(res.data);
      }
    } catch (err) {
      setError('Failed to load loan details.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLoans();
    handleCalculateEmi();
  }, [user]);

  const handleCalculateEmi = async () => {
    try {
      const res = await api.post('/loan/calculate-emi', {
        principalAmount: parseFloat(calcPrincipal),
        annualInterestRate: parseFloat(calcRate),
        tenureMonths: parseInt(calcTenure)
      });
      setCalcResult(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const handleApplyLoan = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    try {
      await api.post('/loan/apply', {
        customerId: user.customerId || 2,
        loanType: loanType,
        principalAmount: parseFloat(appPrincipal),
        tenureMonths: parseInt(appTenure)
      });
      setSuccess('Loan application submitted and passed risk assessment! Pending approval.');
      setShowApplyModal(false);
      fetchLoans();
    } catch (err) {
      setError(err.response?.data?.message || 'Loan application failed.');
    }
  };

  const handleProcessLoan = async (loanId, approve) => {
    setError('');
    setSuccess('');
    try {
      await api.post(`/loan/${loanId}/process`, { approve });
      setSuccess(`Loan #${loanId} successfully ${approve ? 'approved' : 'rejected'}.`);
      fetchLoans();
    } catch (err) {
      setError('Failed to process loan.');
    }
  };

  const handleRepayEmi = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    try {
      await api.post('/loan/repay', {
        loanId: repayLoan.id,
        accountNumber: repayAccount,
        repaymentAmount: parseFloat(repayAmount)
      });
      setSuccess(`Successfully repaid $${repayAmount} for Loan ${repayLoan.loanNumber}.`);
      setRepayLoan(null);
      fetchLoans();
    } catch (err) {
      setError(err.response?.data?.message || 'EMI repayment failed.');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 flex items-center gap-2">
            <Landmark className="w-7 h-7 text-blue-700" /> Loans & EMI Management
          </h1>
          <p className="text-sm text-slate-600 mt-1">
            Scored via ILoanApprovalStrategy & Automated EMI Schedules
          </p>
        </div>

        <button
          onClick={() => setShowApplyModal(true)}
          className="px-4 py-2.5 bg-blue-700 hover:bg-blue-800 text-white rounded-xl text-sm font-bold shadow transition-all flex items-center gap-2"
        >
          <Plus className="w-4 h-4" /> Apply for Loan
        </button>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-2xl text-red-700 text-sm font-semibold flex items-center gap-2">
          <ShieldAlert className="w-5 h-5 flex-shrink-0" /> {error}
        </div>
      )}

      {success && (
        <div className="p-4 bg-emerald-50 border border-emerald-200 rounded-2xl text-emerald-800 text-sm font-semibold flex items-center gap-2">
          <CheckCircle2 className="w-5 h-5 flex-shrink-0" /> {success}
        </div>
      )}

      {/* EMI Calculator Card */}
      <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm">
        <h2 className="text-lg font-bold text-slate-900 mb-4 flex items-center gap-2">
          <Calculator className="w-5 h-5 text-blue-700" /> Interactive EMI Calculator
        </h2>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-xs font-bold text-slate-700 mb-1">Principal Amount ($)</label>
            <input
              type="number"
              value={calcPrincipal}
              onChange={(e) => setCalcPrincipal(e.target.value)}
              className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 mb-1">Annual Interest Rate (%)</label>
            <input
              type="number"
              step="0.1"
              value={calcRate}
              onChange={(e) => setCalcRate(e.target.value)}
              className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 mb-1">Tenure (Months)</label>
            <input
              type="number"
              value={calcTenure}
              onChange={(e) => setCalcTenure(e.target.value)}
              className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
            />
          </div>
        </div>

        <div className="mt-4 pt-4 border-t border-slate-100 flex flex-wrap items-center justify-between gap-4">
          <button
            onClick={handleCalculateEmi}
            className="px-4 py-2 bg-blue-700 hover:bg-blue-800 text-white rounded-xl text-xs font-bold shadow-xs"
          >
            Calculate EMI
          </button>

          {calcResult && (
            <div className="flex gap-6 text-sm font-semibold">
              <div>Monthly EMI: <strong className="text-emerald-700">${calcResult.monthlyEMI}</strong></div>
              <div>Total Interest: <strong className="text-amber-700">${calcResult.totalInterest}</strong></div>
              <div>Total Payment: <strong className="text-slate-900">${calcResult.totalPayment}</strong></div>
            </div>
          )}
        </div>
      </div>

      {/* Loans List Table */}
      <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm">
        <h2 className="text-lg font-bold text-slate-900 mb-4">Loan Accounts & Applications</h2>

        {loans.length === 0 ? (
          <div className="text-center py-8 text-slate-500 text-sm">No loan applications found.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-700">
              <thead className="bg-slate-50 text-xs font-bold uppercase text-slate-500 border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Loan #</th>
                  <th className="py-3 px-4">Customer</th>
                  <th className="py-3 px-4">Type</th>
                  <th className="py-3 px-4">Principal</th>
                  <th className="py-3 px-4">Rate</th>
                  <th className="py-3 px-4">Monthly EMI</th>
                  <th className="py-3 px-4">Remaining</th>
                  <th className="py-3 px-4">Status</th>
                  <th className="py-3 px-4 text-center">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {loans.map((loan) => (
                  <tr key={loan.id} className="hover:bg-slate-50">
                    <td className="py-3 px-4 font-mono text-xs font-bold text-blue-700">{loan.loanNumber}</td>
                    <td className="py-3 px-4 text-xs text-slate-900 font-bold">{loan.customerName}</td>
                    <td className="py-3 px-4 text-xs font-semibold text-slate-700">{loan.loanType}</td>
                    <td className="py-3 px-4 font-bold text-slate-900">${loan.principalAmount.toLocaleString()}</td>
                    <td className="py-3 px-4 text-xs font-medium">{loan.interestRate}%</td>
                    <td className="py-3 px-4 font-bold text-emerald-700">${loan.monthlyEMI}</td>
                    <td className="py-3 px-4 font-bold text-amber-700">${loan.remainingAmount.toLocaleString()}</td>
                    <td className="py-3 px-4">
                      <span className={`px-2.5 py-0.5 rounded-full text-xs font-bold ${
                        loan.status === 'Active' ? 'bg-emerald-50 text-emerald-800 border border-emerald-200' :
                        loan.status === 'Pending' ? 'bg-amber-50 text-amber-800 border border-amber-200' :
                        loan.status === 'Approved' ? 'bg-blue-50 text-blue-800 border border-blue-200' : 'bg-red-50 text-red-800 border border-red-200'
                      }`}>
                        {loan.status}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-center">
                      {loan.status === 'Active' && (
                        <button
                          onClick={() => { setRepayLoan(loan); setRepayAmount(loan.monthlyEMI); }}
                          className="px-3 py-1 bg-emerald-50 hover:bg-emerald-100 text-emerald-800 border border-emerald-200 rounded-lg text-xs font-bold"
                        >
                          Repay EMI
                        </button>
                      )}
                      {user?.role === 'Admin' && loan.status === 'Pending' && (
                        <div className="flex gap-1 justify-center">
                          <button
                            onClick={() => handleProcessLoan(loan.id, true)}
                            className="p-1 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 border border-emerald-200 rounded"
                            title="Approve Loan"
                          >
                            <CheckCircle2 className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => handleProcessLoan(loan.id, false)}
                            className="p-1 bg-red-50 text-red-700 hover:bg-red-100 border border-red-200 rounded"
                            title="Reject Loan"
                          >
                            <XCircle className="w-4 h-4" />
                          </button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Apply Loan Modal */}
      {showApplyModal && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-xs z-50 flex items-center justify-center p-4">
          <div className="bg-white border border-slate-200 rounded-2xl max-w-md w-full p-6 shadow-2xl space-y-4">
            <h3 className="text-xl font-bold text-slate-900 flex items-center gap-2">
              <Landmark className="w-5 h-5 text-blue-700" /> Apply for Loan
            </h3>

            <form onSubmit={handleApplyLoan} className="space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Loan Purpose / Category</label>
                <select
                  value={loanType}
                  onChange={(e) => setLoanType(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
                >
                  <option value="Personal">Personal Loan (11.5% APR)</option>
                  <option value="Home">Home Loan (7.5% APR)</option>
                  <option value="Auto">Auto Loan (8.5% APR)</option>
                  <option value="Business">Business Loan (10.0% APR)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Requested Amount ($)</label>
                <input
                  type="number"
                  required
                  min="500"
                  value={appPrincipal}
                  onChange={(e) => setAppPrincipal(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Tenure (Months)</label>
                <input
                  type="number"
                  required
                  min="6"
                  max="360"
                  value={appTenure}
                  onChange={(e) => setAppTenure(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
                />
              </div>

              <div className="flex gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setShowApplyModal(false)}
                  className="flex-1 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-sm font-bold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="flex-1 py-2 bg-blue-700 hover:bg-blue-800 text-white rounded-xl text-sm font-bold shadow-md"
                >
                  Submit Application
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Repay EMI Modal */}
      {repayLoan && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-xs z-50 flex items-center justify-center p-4">
          <div className="bg-white border border-slate-200 rounded-2xl max-w-md w-full p-6 shadow-2xl space-y-4">
            <h3 className="text-xl font-bold text-slate-900 flex items-center gap-2">
              <DollarSign className="w-5 h-5 text-emerald-600" /> Repay EMI — {repayLoan.loanNumber}
            </h3>

            <form onSubmit={handleRepayEmi} className="space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Debit From Account</label>
                <select
                  value={repayAccount}
                  onChange={(e) => setRepayAccount(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 font-mono focus:outline-none focus:border-blue-700"
                >
                  {accounts.map(acc => (
                    <option key={acc.id} value={acc.accountNumber}>
                      {acc.accountNumber} (Balance: ${acc.balance.toFixed(2)})
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Repayment Amount ($)</label>
                <input
                  type="number"
                  required
                  step="0.01"
                  value={repayAmount}
                  onChange={(e) => setRepayAmount(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
                />
              </div>

              <div className="flex gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setRepayLoan(null)}
                  className="flex-1 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-sm font-bold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="flex-1 py-2 bg-emerald-700 hover:bg-emerald-800 text-white rounded-xl text-sm font-bold shadow-md"
                >
                  Execute EMI Payment
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
